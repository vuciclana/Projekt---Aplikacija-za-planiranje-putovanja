using Microsoft.AspNetCore.Mvc;
using projektLana.Data;

namespace projektLana.Controllers
{
    public class AccommodationsController : Controller
    {
        public IActionResult Index() => View(MockRepository.Accommodations);

        public IActionResult Details(int id)
        {
            var item = MockRepository.Accommodations.FirstOrDefault(a => a.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}