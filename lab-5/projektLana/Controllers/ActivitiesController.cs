using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using projektLana;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace projektLana.Controllers
{
    [Route("entertainments")]
    public class ActivitiesController : Controller
    {
        private readonly AppDbContext _context;

        public ActivitiesController(AppDbContext context)
        {
            _context = context;
        }

        private static string FormatDestinationDisplay(Destination destination)
        {
            return $"{destination.City}, {destination.Country}";
        }

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var slug = Regex.Replace(text.ToLower(), @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            return Regex.Replace(slug, @"-+", "-").Trim('-');
        }

        private bool TryGetTripDateRange(int destinationId, out DateTime startDate, out DateTime endDate)
        {
            var destination = _context.Destinations
                .Include(d => d.Trip)
                .FirstOrDefault(d => d.Id == destinationId && !d.IsDeleted && d.Trip != null && !d.Trip.IsDeleted);

            if (destination?.Trip == null)
            {
                startDate = default;
                endDate = default;
                return false;
            }

            startDate = destination.Trip.StartDate.Date;
            endDate = destination.Trip.EndDate.Date.AddDays(1).AddTicks(-1);
            return true;
        }

        public IActionResult Index()
        {
            var activities = _context.Activities
                .Where(a => !a.IsDeleted)
                .Include(a => a.Destination)
                .OrderBy(a => a.Name)
                .ToList();

            return View(activities);
        }

        [HttpGet("search", Name = "ActivitiesSearch")]
        public IActionResult Search(string? term)
        {
            var search = term?.Trim().ToLower() ?? string.Empty;
            var typeMatches = string.IsNullOrWhiteSpace(search)
                ? new List<ActivityType>()
                : Enum.GetValues<ActivityType>()
                    .Where(type => type.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var query = _context.Activities
                .Where(a => !a.IsDeleted)
                .Include(a => a.Destination)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.Name.ToLower().Contains(search) ||
                    typeMatches.Contains(a.TypeOfActivity) ||
                    (a.Destination != null &&
                     (a.Destination.City.ToLower().Contains(search) ||
                      a.Destination.Country.ToLower().Contains(search))));
            }

            var activities = query.ToList();

            if (string.IsNullOrWhiteSpace(search))
            {
                activities = activities
                    .OrderBy(a => a.Name)
                    .ToList();
            }
            else
            {
                activities = activities
                    .OrderBy(a => GetSearchRank(a, search))
                    .ThenBy(a => a.Name)
                    .ToList();
            }

            return PartialView("_ActivityCardsPartial", activities);
        }

        private static int GetSearchRank(Activity activity, string search)
        {
            var nameRank = GetMatchIndex(activity.Name, search);
            var typeRank = GetMatchIndex(activity.TypeOfActivity.ToString(), search);
            var cityRank = GetMatchIndex(activity.Destination?.City, search);
            var countryRank = GetMatchIndex(activity.Destination?.Country, search);

            return Math.Min(Math.Min(nameRank, typeRank), Math.Min(cityRank, countryRank));
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
            return View(new ActivityFormModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ActivityFormModel model)
        {
            if (model.DestinationId.HasValue && model.Date.HasValue)
            {
                if (TryGetTripDateRange(model.DestinationId.Value, out var startDate, out var endDate))
                {
                    if (model.Date.Value < startDate || model.Date.Value > endDate)
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.Date), $"Date must be within the trip date range ({rangeText}).");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.DestinationId), "Selected destination does not belong to an active trip.");
                }
            }

            if (ModelState.IsValid && model.Date.HasValue && model.Cost.HasValue && model.DestinationId.HasValue && model.TypeOfActivity.HasValue)
            {
                var activity = new Activity
                {
                    Name = model.Name,
                    TypeOfActivity = model.TypeOfActivity.Value,
                    Date = model.Date.Value,
                    Cost = model.Cost.Value,
                    DestinationId = model.DestinationId.Value
                };

                _context.Activities.Add(activity);
                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { activityType = activity.TypeOfActivity.ToString(), activityName = GenerateSlug(activity.Name) });
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
            var activity = _context.Activities
                .Include(a => a.Destination)
                .FirstOrDefault(a => a.Id == id && !a.IsDeleted);
            if (activity == null) return NotFound();

            return View(new ActivityFormModel
            {
                Id = activity.Id,
                Name = activity.Name,
                TypeOfActivity = activity.TypeOfActivity,
                Date = activity.Date,
                Cost = activity.Cost,
                DestinationId = activity.DestinationId,
                DestinationDisplayName = activity.Destination == null ? string.Empty : FormatDestinationDisplay(activity.Destination)
            });
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ActivityFormModel model)
        {
            var activity = _context.Activities.FirstOrDefault(a => a.Id == model.Id && !a.IsDeleted);
            if (activity == null) return NotFound();

            if (model.DestinationId.HasValue && model.Date.HasValue)
            {
                if (TryGetTripDateRange(model.DestinationId.Value, out var startDate, out var endDate))
                {
                    if (model.Date.Value < startDate || model.Date.Value > endDate)
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.Date), $"Date must be within the trip date range ({rangeText}).");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.DestinationId), "Selected destination does not belong to an active trip.");
                }
            }

            if (ModelState.IsValid && model.Date.HasValue && model.Cost.HasValue && model.DestinationId.HasValue && model.TypeOfActivity.HasValue)
            {
                activity.Name = model.Name;
                activity.TypeOfActivity = model.TypeOfActivity.Value;
                activity.Date = model.Date.Value;
                activity.Cost = model.Cost.Value;
                activity.DestinationId = model.DestinationId.Value;

                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { activityType = activity.TypeOfActivity.ToString(), activityName = GenerateSlug(activity.Name) });
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
            var activity = _context.Activities
                .Include(a => a.Destination)
                .FirstOrDefault(a => a.Id == id && !a.IsDeleted);

            if (activity == null) return NotFound();
            return View(activity);
        }

        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var activity = _context.Activities.FirstOrDefault(a => a.Id == id && !a.IsDeleted);
            if (activity == null) return NotFound();

            activity.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{activityType}/{activityName}")]
        public IActionResult Details(string activityType, string activityName)
        {
            var item = _context.Activities
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .ToList()
                .FirstOrDefault(a => !a.IsDeleted &&
                                      a.TypeOfActivity.ToString().ToLower() == activityType.ToLower() && 
                                      GenerateSlug(a.Name) == activityName.ToLower());

            if (item == null) return NotFound();
            return View(item);
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