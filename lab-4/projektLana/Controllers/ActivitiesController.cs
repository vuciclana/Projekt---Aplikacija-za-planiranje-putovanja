using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projektLana.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace projektLana.Controllers
{
    [Route("activities")]
    public class ActivitiesController : Controller
    {
        private readonly AppDbContext _context;

        public ActivitiesController(AppDbContext context)
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
            var activities = _context.Activities
                .Include(a => a.Destination)
                .ToList();

            return View(activities);
        }

        [HttpGet("{activityType}/{activityName}")]
        public IActionResult Details(string activityType, string activityName)
        {
            var item = _context.Activities
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .ToList()
                .FirstOrDefault(a => a.TypeOfActivity.ToString().ToLower() == activityType.ToLower() && 
                                      GenerateSlug(a.Name) == activityName.ToLower());

            if (item == null) return NotFound();
            return View(item);
        }

        [HttpGet("Details/{id}")]
        public IActionResult DetailsByIdFallback(int id)
        {
            var item = _context.Activities
                .Include(a => a.Destination)
                    .ThenInclude(d => d.Trip)
                .FirstOrDefault(a => a.Id == id);

            if (item == null) return NotFound();
            return View("Details", item);
        }
    }
}