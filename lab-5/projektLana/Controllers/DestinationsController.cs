using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using projektLana;
using System.Globalization;
using System.Linq;

namespace projektLana.Controllers
{
    [Route("locations")]
    public class DestinationsController : Controller
    {
        private readonly AppDbContext _context;

        public DestinationsController(AppDbContext context)
        {
            _context = context;
        }

        private static string FormatTripDisplay(Trip trip)
        {
            return trip.Name;
        }

        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            return !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? LocalRedirect(returnUrl)
                : RedirectToAction(nameof(Index));
        }

        private void SetReturnUrl(string? returnUrl)
        {
            ViewData["ReturnUrl"] = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action(nameof(Index));
        }

        public IActionResult Index()
        {
            var destinations = _context.Destinations
                .Where(d => !d.IsDeleted)
                .Include(d => d.Trip)
                .ToList();

            return View(destinations);
        }

        [HttpGet("search")]
        public IActionResult Search(string? term)
        {
            var search = term?.Trim().ToLower() ?? string.Empty;

            var query = _context.Destinations
                .Where(d => !d.IsDeleted)
                .Include(d => d.Trip)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.City.ToLower().Contains(search) ||
                    d.Country.ToLower().Contains(search) ||
                    (d.Trip != null && d.Trip.Name.ToLower().Contains(search)));
            }

            var destinations = query
                .OrderBy(d =>
                    !string.IsNullOrWhiteSpace(search) &&
                    (d.City.ToLower().StartsWith(search) ||
                     d.Country.ToLower().StartsWith(search) ||
                     (d.Trip != null && d.Trip.Name.ToLower().StartsWith(search)))
                        ? 0
                        : 1)
                .ThenBy(d => d.Country)
                .ThenBy(d => d.City)
                .ToList();

            return PartialView("_DestinationCardsPartial", destinations);
        }

        [HttpGet("create")]
        public IActionResult Create(int? tripId, string? returnUrl)
        {
            var model = new DestinationFormModel();
            SetReturnUrl(returnUrl);

            if (tripId.HasValue)
            {
                var selectedTrip = _context.Trips.FirstOrDefault(t => t.Id == tripId.Value && !t.IsDeleted);
                if (selectedTrip != null)
                {
                    model.TripId = selectedTrip.Id;
                    model.TripDisplayName = FormatTripDisplay(selectedTrip);
                }
            }

            return View(model);
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(DestinationFormModel model, string? returnUrl)
        {
            if (ModelState.IsValid && model.TripId.HasValue)
            {
                var destination = new Destination
                {
                    City = model.City,
                    Country = model.Country,
                    Description = model.Description ?? string.Empty,
                    TripId = model.TripId.Value
                };

                _context.Destinations.Add(destination);
                _context.SaveChanges();
                return RedirectToReturnUrl(returnUrl);
            }

            if (model.TripId.HasValue)
            {
                var selectedTrip = _context.Trips.FirstOrDefault(t => t.Id == model.TripId.Value && !t.IsDeleted);
                if (selectedTrip != null)
                {
                    model.TripDisplayName = FormatTripDisplay(selectedTrip);
                }
            }

            SetReturnUrl(returnUrl);
            return View(model);
        }

        [HttpGet("edit/{id:int}")]
        public IActionResult Edit(int id, string? returnUrl)
        {
            var destination = _context.Destinations
                .Include(d => d.Trip)
                .FirstOrDefault(d => d.Id == id && !d.IsDeleted);
            if (destination == null) return NotFound();

            SetReturnUrl(returnUrl);
            return View(new DestinationFormModel
            {
                Id = destination.Id,
                City = destination.City,
                Country = destination.Country,
                Description = destination.Description,
                TripId = destination.TripId,
                TripDisplayName = destination.Trip == null ? string.Empty : FormatTripDisplay(destination.Trip)
            });
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DestinationFormModel model, string? returnUrl)
        {
            var destination = _context.Destinations.FirstOrDefault(d => d.Id == model.Id && !d.IsDeleted);
            if (destination == null) return NotFound();

            if (ModelState.IsValid && model.TripId.HasValue)
            {
                destination.City = model.City;
                destination.Country = model.Country;
                destination.Description = model.Description ?? string.Empty;
                destination.TripId = model.TripId.Value;

                _context.SaveChanges();
                return RedirectToReturnUrl(returnUrl);
            }

            if (model.TripId.HasValue)
            {
                var selectedTrip = _context.Trips.FirstOrDefault(t => t.Id == model.TripId.Value && !t.IsDeleted);
                if (selectedTrip != null)
                {
                    model.TripDisplayName = FormatTripDisplay(selectedTrip);
                }
            }

            SetReturnUrl(returnUrl);
            return View(model);
        }

        [HttpGet("delete/{id:int}")]
        public IActionResult Delete(int id, string? returnUrl)
        {
            var destination = _context.Destinations
                .Include(d => d.Trip)
                .FirstOrDefault(d => d.Id == id && !d.IsDeleted);

            if (destination == null) return NotFound();
            SetReturnUrl(returnUrl);
            return View(destination);
        }

        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id, string? returnUrl)
        {
            var destination = _context.Destinations.FirstOrDefault(d => d.Id == id && !d.IsDeleted);
            if (destination == null) return NotFound();

            destination.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToReturnUrl(returnUrl);
        }

        [HttpGet("{country}/{city}")]
        public IActionResult Details(string country, string city, string? returnUrl)
        {
            var item = _context.Destinations
                .Include(d => d.Trip)
                .Include(d => d.Activities.Where(a => !a.IsDeleted))
                .Include(d => d.Accommodations.Where(a => !a.IsDeleted))
                .Include(d => d.Transports.Where(t => !t.IsDeleted))
                .Include(d => d.Reviews.Where(r => !r.IsDeleted))
                    .ThenInclude(r => r.User)
                .FirstOrDefault(d => !d.IsDeleted &&
                                     d.Country.ToLower() == country.ToLower() &&
                                     d.City.ToLower() == city.ToLower());

            if (item == null) return NotFound();
            SetReturnUrl(returnUrl);
            return View(item);
        }

        [HttpGet("trips")]
        public IActionResult Trips(string? term)
        {
            var search = term?.Trim() ?? string.Empty;

            var query = _context.Trips
                .Where(t => !t.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t => t.Name.Contains(search));
            }

            var results = query
                .OrderBy(t => !string.IsNullOrWhiteSpace(search) && t.Name.StartsWith(search) ? 0 : 1)
                .ThenBy(t => t.Name)
                .Take(10)
                .Select(t => new
                {
                    id = t.Id,
                    text = t.Name
                })
                .ToList();

            return Json(results);
        }

        [HttpGet("trip-range")]
        public IActionResult TripRange(int id)
        {
            var destination = _context.Destinations
                .Include(d => d.Trip)
                .FirstOrDefault(d => d.Id == id && !d.IsDeleted && d.Trip != null && !d.Trip.IsDeleted);

            if (destination?.Trip == null)
            {
                return NotFound();
            }

            var startDate = destination.Trip.StartDate.Date;
            var endDate = destination.Trip.EndDate.Date.AddDays(1).AddTicks(-1);

            return Json(new
            {
                start = startDate.ToString("O", CultureInfo.InvariantCulture),
                end = endDate.ToString("O", CultureInfo.InvariantCulture)
            });
        }
    }
}
