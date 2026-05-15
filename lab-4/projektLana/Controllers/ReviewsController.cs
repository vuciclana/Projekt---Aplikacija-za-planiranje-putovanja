using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using projektLana;
using System.Linq;

namespace projektLana.Controllers
{
    [Route("User-Experiences")]
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        private static string FormatUserDisplay(User user)
        {
            return $"{user.FirstName} {user.LastName} ({user.Email})";
        }

        private static string FormatDestinationDisplay(Destination destination)
        {
            return $"{destination.City}, {destination.Country}";
        }

        public IActionResult Index()
        {
            var reviews = _context.Reviews
                .Where(r => !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Destination)
                .ToList();

            ViewData["CurrentFilter"] = "All";
            return View(reviews);
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            var item = _context.Reviews
                .Where(r => !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Destination)
                    .ThenInclude(d => d.Trip)
                .FirstOrDefault(r => r.Id == id);

            if (item == null) return NotFound();
            return View(item);
        }

        [HttpGet("Recommended")]
        public IActionResult Recommended()
        {
            var reviews = _context.Reviews
                .Where(r => !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Destination)
                .Where(r => r.Rating >= 4)
                .OrderByDescending(r => r.Rating)
                .ThenByDescending(r => r.Id)
                .ToList();

            ViewData["CurrentFilter"] = "Recommended";
            return View("Index", reviews);
        }

        [HttpGet("NeedsImprovements")]
        public IActionResult NeedsImprovements()
        {
            var reviews = _context.Reviews
                .Where(r => !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Destination)
                .Where(r => r.Rating < 4)
                .OrderBy(r => r.Rating)
                .ThenByDescending(r => r.Id)
                .ToList();

            ViewData["CurrentFilter"] = "NeedsImprovements";
            return View("Index", reviews);
        }

        [HttpGet("search", Name = "ReviewsSearch")]
        public IActionResult Search(string? term, string? filter)
        {
            var search = term?.Trim().ToLower() ?? string.Empty;
            var currentFilter = filter ?? "All";

            var query = _context.Reviews
                .Where(r => !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Destination)
                .AsQueryable();

            if (currentFilter == "Recommended")
            {
                query = query.Where(r => r.Rating >= 4);
            }
            else if (currentFilter == "NeedsImprovements")
            {
                query = query.Where(r => r.Rating < 4);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(r =>
                    (r.Comment != null && r.Comment.ToLower().Contains(search)) ||
                    r.Rating.ToString().Contains(search) ||
                    (r.User != null &&
                     ((r.User.FirstName + " " + r.User.LastName).ToLower().Contains(search) ||
                      r.User.Email.ToLower().Contains(search))) ||
                    (r.Destination != null &&
                     (r.Destination.City.ToLower().Contains(search) ||
                      r.Destination.Country.ToLower().Contains(search))));
            }

            var reviews = query.ToList();

            if (string.IsNullOrWhiteSpace(search))
            {
                reviews = reviews
                    .OrderBy(r => r.Destination != null ? r.Destination.City : string.Empty)
                    .ThenBy(r => r.ReviewerName)
                    .ThenByDescending(r => r.Rating)
                    .ToList();
            }
            else
            {
                reviews = reviews
                    .OrderBy(r => GetSearchRank(r, search))
                    .ThenBy(r => r.Destination != null ? r.Destination.City : string.Empty)
                    .ThenBy(r => r.ReviewerName)
                    .ToList();
            }

            ViewData["CurrentFilter"] = currentFilter;
            return PartialView("_ReviewCardsPartial", reviews);
        }

        private static int GetSearchRank(Review review, string search)
        {
            var commentRank = GetMatchIndex(review.Comment, search);
            var ratingRank = GetMatchIndex(review.Rating.ToString(), search);
            var userRank = GetMatchIndex(review.User != null ? review.User.FirstName + " " + review.User.LastName : null, search);
            var emailRank = GetMatchIndex(review.User?.Email, search);
            var cityRank = GetMatchIndex(review.Destination?.City, search);
            var countryRank = GetMatchIndex(review.Destination?.Country, search);

            return Math.Min(Math.Min(commentRank, ratingRank), Math.Min(Math.Min(userRank, emailRank), Math.Min(cityRank, countryRank)));
        }

        private static int GetMatchIndex(string? source, string search)
        {
            if (string.IsNullOrEmpty(source))
            {
                return int.MaxValue;
            }

            var index = source.IndexOf(search, StringComparison.OrdinalIgnoreCase);
            return index < 0 ? int.MaxValue : index;
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new ReviewFormModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ReviewFormModel model)
        {
            if (ModelState.IsValid && model.Rating.HasValue && model.UserId.HasValue && model.DestinationId.HasValue)
            {
                var review = new Review
                {
                    Rating = model.Rating.Value,
                    Comment = model.Comment,
                    UserId = model.UserId.Value,
                    DestinationId = model.DestinationId.Value
                };

                _context.Reviews.Add(review);
                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { id = review.Id });
            }

            if (model.UserId.HasValue)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId.Value);
                if (user != null)
                {
                    model.UserDisplayName = FormatUserDisplay(user);
                }
            }

            if (model.DestinationId.HasValue)
            {
                var destination = _context.Destinations.FirstOrDefault(d => d.Id == model.DestinationId.Value && !d.IsDeleted);
                if (destination != null)
                {
                    model.DestinationDisplayName = FormatDestinationDisplay(destination);
                }
            }

            return View(model);
        }

        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var review = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Destination)
                .FirstOrDefault(r => r.Id == id && !r.IsDeleted);
            if (review == null) return NotFound();

            return View(new ReviewFormModel
            {
                Id = review.Id,
                Rating = review.Rating,
                Comment = review.Comment,
                UserId = review.UserId,
                DestinationId = review.DestinationId,
                UserDisplayName = review.User == null ? string.Empty : FormatUserDisplay(review.User),
                DestinationDisplayName = review.Destination == null ? string.Empty : FormatDestinationDisplay(review.Destination)
            });
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ReviewFormModel model)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.Id == model.Id && !r.IsDeleted);
            if (review == null) return NotFound();

            if (ModelState.IsValid && model.Rating.HasValue && model.UserId.HasValue && model.DestinationId.HasValue)
            {
                review.Rating = model.Rating.Value;
                review.Comment = model.Comment;
                review.UserId = model.UserId.Value;
                review.DestinationId = model.DestinationId.Value;

                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { id = review.Id });
            }

            if (model.UserId.HasValue)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == model.UserId.Value);
                if (user != null)
                {
                    model.UserDisplayName = FormatUserDisplay(user);
                }
            }

            if (model.DestinationId.HasValue)
            {
                var destination = _context.Destinations.FirstOrDefault(d => d.Id == model.DestinationId.Value && !d.IsDeleted);
                if (destination != null)
                {
                    model.DestinationDisplayName = FormatDestinationDisplay(destination);
                }
            }

            return View(model);
        }

        [HttpGet("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var review = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Destination)
                .FirstOrDefault(r => r.Id == id && !r.IsDeleted);
            if (review == null) return NotFound();

            return View(review);
        }

        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.Id == id && !r.IsDeleted);
            if (review == null) return NotFound();

            review.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("users")]
        public IActionResult Users(string? term)
        {
            var search = term?.Trim() ?? string.Empty;

            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    (u.FirstName + " " + u.LastName).Contains(search) ||
                    u.Email.Contains(search));
            }

            var results = query
                .OrderBy(u =>
                    !string.IsNullOrWhiteSpace(search) &&
                    ((u.FirstName + " " + u.LastName).StartsWith(search) ||
                     u.Email.StartsWith(search)) ? 0 : 1)
                .ThenBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .Take(10)
                .Select(u => new
                {
                    id = u.Id,
                    text = u.FirstName + " " + u.LastName + " (" + u.Email + ")"
                })
                .ToList();

            return Json(results);
        }

        [HttpGet("destinations")]
        public IActionResult Destinations(string? term)
        {
            var search = term?.Trim() ?? string.Empty;

            var query = _context.Destinations
                .Where(d => !d.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.City.Contains(search) ||
                    d.Country.Contains(search));
            }

            var results = query
                .OrderBy(d =>
                    !string.IsNullOrWhiteSpace(search) &&
                    (d.City.StartsWith(search) || d.Country.StartsWith(search))
                        ? 0
                        : 1)
                .ThenBy(d => d.Country)
                .ThenBy(d => d.City)
                .Take(10)
                .Select(d => new
                {
                    id = d.Id,
                    text = d.City + ", " + d.Country
                })
                .ToList();

            return Json(results);
        }
    }
}