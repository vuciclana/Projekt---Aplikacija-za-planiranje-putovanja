using Microsoft.AspNetCore.Mvc;
using projektLana.Data;

namespace projektLana.Controllers
{
    public class DestinationsController : Controller
    {
        public IActionResult Index() => View(MockRepository.Destinations);

        public IActionResult Details(int id)
        {
            var item = MockRepository.Destinations.FirstOrDefault(d => d.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}