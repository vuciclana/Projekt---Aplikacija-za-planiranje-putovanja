using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using projektLana;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace projektLana.Controllers
{
    [Route("locations")]
    public class DestinationsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;  //objekt s kojim se može pristupiti informacijama o web hostingu, uključujući put do root direktorija web aplikacije
        private const long MaxPhotoSize = 10 * 1024 * 1024;
        private static readonly HashSet<string> AllowedPhotoExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".gif", ".webp"
        };

        public DestinationsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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
        //podaci o putu do root direktorija web aplikacije
        private string WebRootPath =>
            _environment.WebRootPath ??
            Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot");

        private static string GetSafeFileName(string fileName)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            name = Regex.Replace(name, @"[^a-zA-Z0-9_-]+", "-").Trim('-');
            return string.IsNullOrWhiteSpace(name) ? "photo" : name;
        }

        private static object ToPhotoJson(DestinationPhoto photo)
        {
            return new
            {
                id = photo.Id,
                name = photo.OriginalFileName,
                url = photo.FilePath,
                size = photo.FileSize,
                uploadedAt = photo.UploadedAt.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)
            };
        }

        private async Task AttachPendingPhotosAsync(int destinationId, Guid uploadSessionId)
        {
            var pendingPhotos = await _context.DestinationPhotos
                .Where(p => p.UploadSessionId == uploadSessionId && p.DestinationId == null)
                .ToListAsync();

            if (!pendingPhotos.Any())
            {
                return;
            }

            var destinationFolder = Path.Combine(WebRootPath, "uploads", "destinations", destinationId.ToString(CultureInfo.InvariantCulture));
            Directory.CreateDirectory(destinationFolder);

            foreach (var photo in pendingPhotos)
            {
                var oldRelativePath = photo.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var oldFullPath = Path.Combine(WebRootPath, oldRelativePath);
                var newFullPath = Path.Combine(destinationFolder, photo.StoredFileName);

                if (System.IO.File.Exists(oldFullPath))
                {
                    if (System.IO.File.Exists(newFullPath))
                    {
                        System.IO.File.Delete(newFullPath);
                    }

                    System.IO.File.Move(oldFullPath, newFullPath);
                }

                photo.DestinationId = destinationId;
                photo.UploadSessionId = null;
                photo.FilePath = $"/uploads/destinations/{destinationId}/{photo.StoredFileName}";
            }

            await _context.SaveChangesAsync();
        }

        public IActionResult Index()
        {
            var destinations = _context.Destinations
                .Where(d => !d.IsDeleted)
                .Include(d => d.Trip)
                .Include(d => d.Photos.OrderBy(p => p.UploadedAt))
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
                .Include(d => d.Photos.OrderBy(p => p.UploadedAt))
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
        public async Task<IActionResult> Create(DestinationFormModel model, string? returnUrl)
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
                await _context.SaveChangesAsync();
                await AttachPendingPhotosAsync(destination.Id, model.UploadSessionId.GetValueOrDefault());
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
                TripDisplayName = destination.Trip == null ? string.Empty : FormatTripDisplay(destination.Trip),
                UploadSessionId = Guid.NewGuid()
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

        [HttpGet("{id:int}/photos")]
        public async Task<IActionResult> Photos(int id)
        {
            var destinationExists = await _context.Destinations.AnyAsync(d => d.Id == id && !d.IsDeleted);
            if (!destinationExists)
            {
                return NotFound();
            }

            var photos = await _context.DestinationPhotos
                .Where(p => p.DestinationId == id)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();

            return Json(photos.Select(ToPhotoJson));
        }

        [HttpGet("photos/pending")]
        public async Task<IActionResult> PendingPhotos(Guid uploadSessionId)
        {
            if (uploadSessionId == Guid.Empty)
            {
                return BadRequest();
            }

            var photos = await _context.DestinationPhotos
                .Where(p => p.UploadSessionId == uploadSessionId && p.DestinationId == null)
                .OrderByDescending(p => p.UploadedAt)
                .ToListAsync();

            return Json(photos.Select(ToPhotoJson));
        }

        [HttpPost("photos/upload")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadPhoto(int? id, Guid uploadSessionId, IFormFile? file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { error = "Select a photo to upload." });
            }

            if (file.Length > MaxPhotoSize)
            {
                return BadRequest(new { error = "Photos must be 10 MB or smaller." });
            }

            var extension = Path.GetExtension(file.FileName);
            if (!AllowedPhotoExtensions.Contains(extension) || !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { error = "Only JPG, PNG, GIF, and WebP images are allowed." });
            }

            Destination? destination = null;
            if (id.HasValue)
            {
                destination = await _context.Destinations.FirstOrDefaultAsync(d => d.Id == id.Value && !d.IsDeleted);
                if (destination == null)
                {
                    return NotFound();
                }
            }
            else if (uploadSessionId == Guid.Empty)
            {
                return BadRequest(new { error = "Missing upload session." });
            }

            var storedFileName = $"{GetSafeFileName(file.FileName)}-{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            var relativeFolder = destination == null
                ? $"uploads/destinations/pending/{uploadSessionId}"
                : $"uploads/destinations/{destination.Id}";
            var uploadFolder = Path.Combine(WebRootPath, relativeFolder.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(uploadFolder);

            var fullPath = Path.Combine(uploadFolder, storedFileName);
            await using (var stream = System.IO.File.Create(fullPath))
            {
                await file.CopyToAsync(stream);
            }

            var photo = new DestinationPhoto
            {
                OriginalFileName = Path.GetFileName(file.FileName),
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FilePath = $"/{relativeFolder}/{storedFileName}",
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow,
                DestinationId = destination?.Id,
                UploadSessionId = destination == null ? uploadSessionId : null
            };

            _context.DestinationPhotos.Add(photo);
            await _context.SaveChangesAsync();

            return Json(ToPhotoJson(photo));
        }

        [HttpDelete("photos/{photoId:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePhoto(int photoId, Guid? uploadSessionId)
        {
            var photo = await _context.DestinationPhotos
                .Include(p => p.Destination)
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo == null)
            {
                return NotFound();
            }

            var isPendingOwner = uploadSessionId.HasValue &&
                                 uploadSessionId.Value != Guid.Empty &&
                                 photo.UploadSessionId == uploadSessionId.Value &&
                                 photo.DestinationId == null;
            var isDestinationPhoto = photo.DestinationId.HasValue &&
                                     photo.Destination != null &&
                                     !photo.Destination.IsDeleted;

            if (!isPendingOwner && !isDestinationPhoto)
            {
                return NotFound();
            }

            var relativePath = photo.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(WebRootPath, relativePath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            _context.DestinationPhotos.Remove(photo);
            await _context.SaveChangesAsync();

            return Ok(new { deleted = true });
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
                .Include(d => d.Photos.OrderBy(p => p.UploadedAt))
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
