using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;

namespace projektLana.Controllers
{
    public class AccommodationsController : Controller
    {
        private readonly AppDbContext _context;

        public AccommodationsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var accommodations = _context.Accommodations
                .Include(a => a.Destination)
                .ToList();

            return View(accommodations);
        }

        public IActionResult Details(int id)
        {
            var item = _context.Accommodations
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .FirstOrDefault(a => a.Id == id);

            if (item == null) return NotFound();
            return View(item);
        }
    }
}