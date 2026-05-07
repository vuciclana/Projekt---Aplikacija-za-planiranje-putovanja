using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;

namespace projektLana.Controllers
{
    public class TransportsController : Controller
    {
        private readonly AppDbContext _context;

        public TransportsController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var transports = _context.Transports
                .Include(t => t.Destination)
                .ToList();

            return View(transports);
        }

        public IActionResult Details(int id)
        {
            var item = _context.Transports
                .Include(t => t.Destination)
                    .ThenInclude(d => d.Trip)
                .FirstOrDefault(t => t.Id == id);

            if (item == null) return NotFound();
            return View(item);
        }
    }
}