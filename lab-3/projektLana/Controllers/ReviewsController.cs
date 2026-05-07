using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;

namespace projektLana.Controllers
{
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

            return View(reviews);
        }

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
    }
}