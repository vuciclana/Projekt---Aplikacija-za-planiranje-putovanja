using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
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

        private string GenerateSlug(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            var slug = Regex.Replace(text.ToLower(), @"[^a-z0-9\s-]", "");
            slug = Regex.Replace(slug, @"\s+", "-");
            return Regex.Replace(slug, @"-+", "-").Trim('-');
        }

        public IActionResult Index()
        {
            var trips = _context.Trips
                .Include(t => t.User)
                .Include(t => t.Destinations)
                .ToList();

            return View(trips);
        }

        [HttpGet("{slug}")]
        public IActionResult Details(string slug)
        {
            var trip = _context.Trips
                .Include(t => t.User)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Activities)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Accommodations)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Transports)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Reviews)
                .ToList()
                .FirstOrDefault(t => GenerateSlug(t.Name) == slug.ToLower());

            if (trip == null) return NotFound();
            return View(trip);
        }

        [HttpGet("Details/{id}")]
        public IActionResult DetailsByIdFallback(int id)
        {
            var trip = _context.Trips
                .Include(t => t.User)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Activities)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Accommodations)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Transports)
                .Include(t => t.Destinations)
                    .ThenInclude(d => d.Reviews)
                .FirstOrDefault(t => t.Id == id);

            if (trip == null) return NotFound();
            return View("Details", trip);
        }
    }
}