using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
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

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var slug = Regex.Replace(text.ToLower(), @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            return Regex.Replace(slug, @"-+", "-").Trim('-');
        }

        public IActionResult Index()
        {
            var accommodations = _context.Accommodations
                .Include(a => a.Destination)
                .ToList();

            return View(accommodations);
        }

        [HttpGet("{accommodationType}/{accommodationName}")]
        public IActionResult Details(string accommodationType, string accommodationName)
        {
            var item = _context.Accommodations
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .ToList()
                .FirstOrDefault(a => a.Type.ToString().ToLower() == accommodationType.ToLower() && 
                                      GenerateSlug(a.Name) == accommodationName.ToLower());

            if (item == null) return NotFound();
            return View(item);
        }

        [HttpGet("Details/{id}")]
        public IActionResult DetailsByIdFallback(int id)
        {
            var item = _context.Accommodations
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .FirstOrDefault(a => a.Id == id);

            if (item == null) return NotFound();
            return View("Details", item);
        }
    }
}