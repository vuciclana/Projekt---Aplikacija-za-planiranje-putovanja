using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using projektLana;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace projektLana.Controllers
{
    [Route("travel")]
    public class TripsController : Controller
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        private static string FormatUserDisplay(User user)
        {
            return $"{user.FirstName} {user.LastName} ({user.Email})";
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var slug = Regex.Replace(text.ToLower(), @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            return Regex.Replace(slug, @"-+", "-").Trim('-');
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var trips = _context.Trips
                .Where(t => !t.IsDeleted)
                .Include(t => t.User)
                .Include(t => t.Destinations)
                .ToList();

            return View(trips);
        }

        [HttpGet("search")]
        public IActionResult Search(string? term)
        {
            var search = term?.Trim().ToLower() ?? string.Empty;

            var query = _context.Trips
                .Where(t => !t.IsDeleted)
                .Include(t => t.User)
                .Include(t => t.Destinations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    t.Name.ToLower().Contains(search) ||
                    (t.User != null &&
                     (t.User.FirstName + " " + t.User.LastName).ToLower().Contains(search)) ||
                    (t.User != null && t.User.Email.ToLower().Contains(search)));
            }

            var trips = query
                .OrderBy(t =>
                    !string.IsNullOrWhiteSpace(search) &&
                    (t.Name.ToLower().StartsWith(search) ||
                     (t.User != null &&
                      ((t.User.FirstName + " " + t.User.LastName).ToLower().StartsWith(search) ||
                       t.User.Email.ToLower().StartsWith(search))))
                        ? 0
                        : 1)
                .ThenBy(t => t.Name)
                .ToList();

            return PartialView("_TripCardsPartial", trips);
        }

        [HttpGet("{slug}")]
        public IActionResult Details(string slug)
        {
            var trip = _context.Trips
                .Include(t => t.User)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Photos)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Activities)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Accommodations)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Transports)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Reviews)
                .ToList()
                .FirstOrDefault(t => GenerateSlug(t.Name) == slug.ToLower() && !t.IsDeleted);

            if (trip == null) return NotFound();
            return View(trip);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View(new TripFormModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TripFormModel model)
        {
            if (!model.StartDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.StartDate), "Start date is required.");
            }

            if (!model.EndDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End date is required.");
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            if (ModelState.IsValid && model.StartDate.HasValue && model.EndDate.HasValue && model.UserId.HasValue)
            {
                var trip = new Trip
                {
                    Name = model.Name,
                    StartDate = model.StartDate.Value,
                    EndDate = model.EndDate.Value,
                    UserId = model.UserId.Value
                };

                _context.Trips.Add(trip);
                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { slug = GenerateSlug(trip.Name) });
            }

            if (model.UserId.HasValue)
            {
                var selectedUser = _context.Users.FirstOrDefault(u => u.Id == model.UserId.Value);
                if (selectedUser != null)
                {
                    model.UserDisplayName = FormatUserDisplay(selectedUser);
                }
            }
            return View(model);
        }

        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (trip == null) return NotFound();

            var selectedUser = _context.Users.FirstOrDefault(u => u.Id == trip.UserId);
            return View(new TripFormModel
            {
                Id = trip.Id,
                Name = trip.Name,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                UserId = trip.UserId,
                UserDisplayName = selectedUser == null ? string.Empty : FormatUserDisplay(selectedUser)
            });
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TripFormModel model)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.Id == model.Id && !t.IsDeleted);
            if (trip == null) return NotFound();

            if (!model.StartDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.StartDate), "Start date is required.");
            }

            if (!model.EndDate.HasValue)
            {
                ModelState.AddModelError(nameof(model.EndDate), "End date is required.");
            }

            if (model.StartDate.HasValue && model.EndDate.HasValue && model.EndDate <= model.StartDate)
            {
                ModelState.AddModelError("EndDate", "End date must be after the start date.");
            }

            if (ModelState.IsValid && model.StartDate.HasValue && model.EndDate.HasValue && model.UserId.HasValue)
            {
                trip.Name = model.Name;
                trip.StartDate = model.StartDate.Value;
                trip.EndDate = model.EndDate.Value;
                trip.UserId = model.UserId.Value;

                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { slug = GenerateSlug(trip.Name) });
            }

            if (model.UserId.HasValue)
            {
                var selectedUser = _context.Users.FirstOrDefault(u => u.Id == model.UserId.Value);
                if (selectedUser != null)
                {
                    model.UserDisplayName = FormatUserDisplay(selectedUser);
                }
            }
            return View(model);
        }           

        [HttpGet("users")]
        public IActionResult Users(string? term)
        {
            var query = _context.Users.AsQueryable();
            var search = term?.Trim() ?? "";

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

        [HttpGet("delete/{id:int}")]
        public IActionResult Delete(int id)
        {
            var trip = _context.Trips
                .Include(t => t.User)
                .Include(t => t.Destinations)
                .FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            
            if (trip == null) return NotFound();
            return View(trip);
        }

        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]  
        public IActionResult DeletePost(int id)
        {
            var trip = _context.Trips.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (trip == null) return NotFound();

            trip.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}
