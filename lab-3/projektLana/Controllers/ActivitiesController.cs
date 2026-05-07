using Microsoft.AspNetCore.Mvc;
using projektLana.Data;

namespace projektLana.Controllers
{
    public class ActivitiesController : Controller
    {
        public IActionResult Index() => View(MockRepository.Activities);

        public IActionResult Details(int id)
        {
            var item = MockRepository.Activities.FirstOrDefault(a => a.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}