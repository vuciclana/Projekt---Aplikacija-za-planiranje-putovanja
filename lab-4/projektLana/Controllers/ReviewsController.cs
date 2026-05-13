using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;

namespace projektLana.Controllers
{
    [Route("Reviews")]
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var reviews = _context.Reviews
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
                .Include(r => r.User)
                .Include(r => r.Destination)
                .Where(r => r.Rating < 4)
                .OrderBy(r => r.Rating)
                .ThenByDescending(r => r.Id)
                .ToList();

            ViewData["CurrentFilter"] = "NeedsImprovements";
            return View("Index", reviews);
        }
    }
}