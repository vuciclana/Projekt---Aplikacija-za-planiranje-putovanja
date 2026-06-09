using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using projektLana;
using System;
using System.Globalization;
using System.Linq;

namespace projektLana.Controllers
{
    [Route("transports")]
    public class TransportsController : Controller
    {
        private readonly AppDbContext _context;

        public TransportsController(AppDbContext context)
        {
            _context = context;
        }

        private static string FormatDestinationDisplay(Destination destination)
        {
            return $"{destination.City}, {destination.Country}";
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
            var transports = _context.Transports
                .Where(t => !t.IsDeleted)
                .Include(t => t.Destination)
                .OrderBy(t => t.Type)
                .ThenBy(t => t.Destination != null ? t.Destination.City : string.Empty)
                .ToList();

            return View(transports);
        }

        [HttpGet("search", Name = "TransportsSearch")]
        public IActionResult Search(string? term)
        {
            var search = term?.Trim().ToLower() ?? string.Empty;
            var typeMatches = string.IsNullOrWhiteSpace(search)
                ? new List<TransportType>()
                : Enum.GetValues<TransportType>()
                    .Where(type => type.ToString().Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            var query = _context.Transports
                .Where(t => !t.IsDeleted)
                .Include(t => t.Destination)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(t =>
                    typeMatches.Contains(t.Type) ||
                    (t.Destination != null &&
                     (t.Destination.City.ToLower().Contains(search) ||
                      t.Destination.Country.ToLower().Contains(search))));
            }

            var transports = query.ToList();

            if (string.IsNullOrWhiteSpace(search))
            {
                transports = transports
                    .OrderBy(t => t.Type)
                    .ThenBy(t => t.Destination != null ? t.Destination.City : string.Empty)
                    .ToList();
            }
            else
            {
                transports = transports
                    .OrderBy(t => GetSearchRank(t, search))
                    .ThenBy(t => t.Type)
                    .ThenBy(t => t.Destination != null ? t.Destination.City : string.Empty)
                    .ToList();
            }

            return PartialView("_TransportCardsPartial", transports);
        }

        private static int GetSearchRank(Transport transport, string search)
        {
            var typeRank = GetMatchIndex(transport.Type.ToString(), search);
            var cityRank = GetMatchIndex(transport.Destination?.City, search);
            var countryRank = GetMatchIndex(transport.Destination?.Country, search);

            return Math.Min(typeRank, Math.Min(cityRank, countryRank));
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
            return View(new TransportFormModel());
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(TransportFormModel model)
        {
            if (model.DepartureTime.HasValue && model.ArrivalTime.HasValue && model.ArrivalTime <= model.DepartureTime)
            {
                ModelState.AddModelError(nameof(model.ArrivalTime), "Arrival time must be after departure time.");
            }

            if (model.DestinationId.HasValue)
            {
                if (TryGetTripDateRange(model.DestinationId.Value, out var startDate, out var endDate))
                {
                    if (model.DepartureTime.HasValue && (model.DepartureTime.Value < startDate || model.DepartureTime.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.DepartureTime), $"Departure time must be within the trip date range ({rangeText}).");
                    }

                    if (model.ArrivalTime.HasValue && (model.ArrivalTime.Value < startDate || model.ArrivalTime.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.ArrivalTime), $"Arrival time must be within the trip date range ({rangeText}).");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.DestinationId), "Selected destination does not belong to an active trip.");
                }
            }

            if (ModelState.IsValid && model.Type.HasValue && model.Cost.HasValue &&
                model.DepartureTime.HasValue && model.ArrivalTime.HasValue && model.DestinationId.HasValue)
            {
                var transport = new Transport
                {
                    Type = model.Type.Value,
                    Cost = model.Cost.Value,
                    DepartureTime = model.DepartureTime.Value,
                    ArrivalTime = model.ArrivalTime.Value,
                    DestinationId = model.DestinationId.Value
                };

                _context.Transports.Add(transport);
                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { id = transport.Id });
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
            var transport = _context.Transports
                .Include(t => t.Destination)
                .FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (transport == null) return NotFound();

            return View(new TransportFormModel
            {
                Id = transport.Id,
                Type = transport.Type,
                Cost = transport.Cost,
                DepartureTime = transport.DepartureTime,
                ArrivalTime = transport.ArrivalTime,
                DestinationId = transport.DestinationId,
                DestinationDisplayName = transport.Destination == null ? string.Empty : FormatDestinationDisplay(transport.Destination)
            });
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(TransportFormModel model)
        {
            var transport = _context.Transports.FirstOrDefault(t => t.Id == model.Id && !t.IsDeleted);
            if (transport == null) return NotFound();

            if (model.DepartureTime.HasValue && model.ArrivalTime.HasValue && model.ArrivalTime <= model.DepartureTime)
            {
                ModelState.AddModelError(nameof(model.ArrivalTime), "Arrival time must be after departure time.");
            }

            if (model.DestinationId.HasValue)
            {
                if (TryGetTripDateRange(model.DestinationId.Value, out var startDate, out var endDate))
                {
                    if (model.DepartureTime.HasValue && (model.DepartureTime.Value < startDate || model.DepartureTime.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.DepartureTime), $"Departure time must be within the trip date range ({rangeText}).");
                    }

                    if (model.ArrivalTime.HasValue && (model.ArrivalTime.Value < startDate || model.ArrivalTime.Value > endDate))
                    {
                        var rangeText = $"{startDate.ToString("d", CultureInfo.CurrentCulture)} - {endDate.ToString("d", CultureInfo.CurrentCulture)}";
                        ModelState.AddModelError(nameof(model.ArrivalTime), $"Arrival time must be within the trip date range ({rangeText}).");
                    }
                }
                else
                {
                    ModelState.AddModelError(nameof(model.DestinationId), "Selected destination does not belong to an active trip.");
                }
            }

            if (ModelState.IsValid && model.Type.HasValue && model.Cost.HasValue &&
                model.DepartureTime.HasValue && model.ArrivalTime.HasValue && model.DestinationId.HasValue)
            {
                transport.Type = model.Type.Value;
                transport.Cost = model.Cost.Value;
                transport.DepartureTime = model.DepartureTime.Value;
                transport.ArrivalTime = model.ArrivalTime.Value;
                transport.DestinationId = model.DestinationId.Value;

                _context.SaveChanges();
                return RedirectToAction(nameof(Details), new { id = transport.Id });
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
            var transport = _context.Transports
                .Include(t => t.Destination)
                .FirstOrDefault(t => t.Id == id && !t.IsDeleted);

            if (transport == null) return NotFound();
            return View(transport);
        }

        [HttpPost("delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var transport = _context.Transports.FirstOrDefault(t => t.Id == id && !t.IsDeleted);
            if (transport == null) return NotFound();

            transport.IsDeleted = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            var item = _context.Transports
                .Include(t => t.Destination)
                    .ThenInclude(d => d.Trip)
                .FirstOrDefault(t => t.Id == id && !t.IsDeleted);

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