using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;

namespace projektLana.Controllers
{
    [Route("Destinations")]
    public class DestinationsController : Controller
    {
        private readonly AppDbContext _context;

        public DestinationsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var destinations = _context.Destinations
                .Include(d => d.Trip)
                .ToList();

            return View(destinations);
        }

        [HttpGet("{country}/{city}")]
        public IActionResult Details(string country, string city)
        {
            var item = _context.Destinations
                .Include(d => d.Trip)
                .Include(d => d.Activities)
                .Include(d => d.Accommodations)
                .Include(d => d.Transports)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(d => d.Country.ToLower() == country.ToLower() && d.City.ToLower() == city.ToLower());

            if (item == null) return NotFound();
            return View(item);
        }

        [HttpGet("Details/{id}")]
        public IActionResult DetailsByIdFallback(int id)
        {
            var item = _context.Destinations
                .Include(d => d.Trip)
                .Include(d => d.Activities)
                .Include(d => d.Accommodations)
                .Include(d => d.Transports)
                .Include(d => d.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefault(d => d.Id == id);

            if (item == null) return NotFound();
            return View("Details", item);
        }
    }
}