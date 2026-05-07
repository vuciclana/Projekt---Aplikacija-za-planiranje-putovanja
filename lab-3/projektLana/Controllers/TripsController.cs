using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;
using System.Threading.Tasks;

namespace projektLana.Controllers
{
    public class TripsController : Controller
    {
        private readonly AppDbContext _context;

        public TripsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var trips = _context.Trips
                .Include(t => t.User)
                .Include(t => t.Destinations)
                .ToList();

            return View(trips);
        }

        public IActionResult Details(int id)
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
            return View(trip);
        }
    }
}