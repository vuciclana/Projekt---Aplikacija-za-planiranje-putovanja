using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;

namespace projektLana.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly AppDbContext _context;

        public ActivitiesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var activities = _context.Activities
                .Include(a => a.Destination)
                .ToList();

            return View(activities);
        }

        public IActionResult Details(int id)
        {
            var item = _context.Activities
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .FirstOrDefault(a => a.Id == id);

            if (item == null) return NotFound();
            return View(item);
        }
    }
}