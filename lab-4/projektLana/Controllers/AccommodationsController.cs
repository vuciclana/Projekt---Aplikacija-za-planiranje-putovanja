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
    [Route("stays")]
    public class AccommodationsController : Controller
    {
        private readonly AppDbContext _context;

        public AccommodationsController(AppDbContext context)
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
            var accommodations = _context.Accommodations
                .Where(a => !a.IsDeleted)
                .Include(a => a.Destination)
                .OrderBy(a => a.Name)
                .ToList();

            return View(accommodations);
        }

        [HttpGet("search", Name = "AccommodationsSearch")]
        public IActionResult Search(string? term)
        {
            var search = term?.Trim().ToLower() ?? string.Empty;
            var typeMatches = string.IsNullOrWhiteSpace(search)
                ? new List<AccommodationType>()
                : Enum.GetValues<AccommodationType>()
                    .Where(type => type.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var query = _context.Accommodations
                .Where(a => !a.IsDeleted)
                .Include(a => a.Destination)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(a =>
                    a.Name.ToLower().Contains(search) ||
                    typeMatches.Contains(a.Type) ||
                    a.Address.ToLower().Contains(search) ||
                    (a.Destination != null &&
                     (a.Destination.City.ToLower().Contains(search) ||
                      a.Destination.Country.ToLower().Contains(search))));
            }

            var accommodations = query.ToList();

            if (string.IsNullOrWhiteSpace(search))
            {
                accommodations = accommodations
                    .OrderBy(a => a.Name)
                    .ToList();
            }
            else
            {
                accommodations = accommodations
                    .OrderBy(a => GetSearchRank(a, search))
                    .ThenBy(a => a.Name)
                    .ToList();
            }

            return PartialView("_AccommodationCardsPartial", accommodations);
        }

        private static int GetSearchRank(Accommodation accommodation, string search)
        {
            var nameRank = GetMatchIndex(accommodation.Name, search);
            var typeRank = GetMatchIndex(accommodation.Type.ToString(), search);
            var addressRank = GetMatchIndex(accommodation.Address, search);
            var cityRank = GetMatchIndex(accommodation.Destination?.City, search);
            var countryRank = GetMatchIndex(accommodation.Destination?.Country, search);

            return Math.Min(Math.Min(nameRank, typeRank), Math.Min(addressRank, Math.Min(cityRank, countryRank)));
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
            return View(new AccommodationFormModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AccommodationFormModel model)
        {
            if (model.CheckInDate.HasValue && model.CheckOutDate.HasValue && model.CheckOutDate <= model.CheckInDate)
            {
                ModelState.AddModelError(nameof(model.CheckOutDate), "Check-out date must be after check-in date.");
            }

            if (model.DestinationId.HasValue)
            {
                if (TryGetTripDateRange(model.DestinationId.Value, out var startDate, out var endDate))
                {
                    if (model.CheckInDate.HasValue && (model.CheckInDate.Value < startDate || model.CheckInDate.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.CheckInDate), $"Check-in date must be within the trip date range ({rangeText}).");
                    }

                    if (model.CheckOutDate.HasValue && (model.CheckOutDate.Value < startDate || model.CheckOutDate.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.CheckOutDate), $"Check-out date must be within the trip date range ({rangeText}).");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.DestinationId), "Selected destination does not belong to an active trip.");
                }
            }

            if (ModelState.IsValid && model.Type.HasValue && model.CostPerNight.HasValue &&
                model.CheckInDate.HasValue && model.CheckOutDate.HasValue && model.DestinationId.HasValue)
            {
                var accommodation = new Accommodation
                {
                    Name = model.Name,
                    Type = model.Type.Value,
                    Address = model.Address,
                    CostPerNight = model.CostPerNight.Value,
                    CheckInDate = model.CheckInDate.Value,
                    CheckOutDate = model.CheckOutDate.Value,
                    DestinationId = model.DestinationId.Value
                };

                _context.Accommodations.Add(accommodation);
                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { accommodationType = accommodation.Type.ToString(), accommodationName = GenerateSlug(accommodation.Name) });
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
            var accommodation = _context.Accommodations
                .Include(a => a.Destination)
                .FirstOrDefault(a => a.Id == id && !a.IsDeleted);
            if (accommodation == null) return NotFound();

            return View(new AccommodationFormModel
            {
                Id = accommodation.Id,
                Name = accommodation.Name,
                Type = accommodation.Type,
                Address = accommodation.Address,
                CostPerNight = accommodation.CostPerNight,
                CheckInDate = accommodation.CheckInDate,
                CheckOutDate = accommodation.CheckOutDate,
                DestinationId = accommodation.DestinationId,
                DestinationDisplayName = accommodation.Destination == null ? string.Empty : FormatDestinationDisplay(accommodation.Destination)
            });
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(AccommodationFormModel model)
        {
            var accommodation = _context.Accommodations.FirstOrDefault(a => a.Id == model.Id && !a.IsDeleted);
            if (accommodation == null) return NotFound();

            if (model.CheckInDate.HasValue && model.CheckOutDate.HasValue && model.CheckOutDate <= model.CheckInDate)
            {
                ModelState.AddModelError(nameof(model.CheckOutDate), "Check-out date must be after check-in date.");
            }

            if (model.DestinationId.HasValue)
            {
                if (TryGetTripDateRange(model.DestinationId.Value, out var startDate, out var endDate))
                {
                    if (model.CheckInDate.HasValue && (model.CheckInDate.Value < startDate || model.CheckInDate.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.CheckInDate), $"Check-in date must be within the trip date range ({rangeText}).");
                    }

                    if (model.CheckOutDate.HasValue && (model.CheckOutDate.Value < startDate || model.CheckOutDate.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.CheckOutDate), $"Check-out date must be within the trip date range ({rangeText}).");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.DestinationId), "Selected destination does not belong to an active trip.");
                }
            }

            if (ModelState.IsValid && model.Type.HasValue && model.CostPerNight.HasValue &&
                model.CheckInDate.HasValue && model.CheckOutDate.HasValue && model.DestinationId.HasValue)
            {
                accommodation.Name = model.Name;
                accommodation.Type = model.Type.Value;
                accommodation.Address = model.Address;
                accommodation.CostPerNight = model.CostPerNight.Value;
                accommodation.CheckInDate = model.CheckInDate.Value;
                accommodation.CheckOutDate = model.CheckOutDate.Value;
                accommodation.DestinationId = model.DestinationId.Value;

                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { accommodationType = accommodation.Type.ToString(), accommodationName = GenerateSlug(accommodation.Name) });
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
            var accommodation = _context.Accommodations
                .Include(a => a.Destination)
                .FirstOrDefault(a => a.Id == id && !a.IsDeleted);

            if (accommodation == null) return NotFound();
            return View(accommodation);
        }

        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var accommodation = _context.Accommodations.FirstOrDefault(a => a.Id == id && !a.IsDeleted);
            if (accommodation == null) return NotFound();

            accommodation.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{accommodationType}/{accommodationName}")]
        public IActionResult Details(string accommodationType, string accommodationName)
        {
            var item = _context.Accommodations
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .ToList()
                .FirstOrDefault(a => !a.IsDeleted &&
                                      a.Type.ToString().ToLower() == accommodationType.ToLower() && 
                                      GenerateSlug(a.Name) == accommodationName.ToLower());

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