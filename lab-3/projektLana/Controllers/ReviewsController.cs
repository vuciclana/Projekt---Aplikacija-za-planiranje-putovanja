using Microsoft.AspNetCore.Mvc;
using projektLana.Data;

namespace projektLana.Controllers
{
    public class ReviewsController : Controller
    {
        public IActionResult Index() => View(MockRepository.Reviews);

        public IActionResult Details(int id)
        {
            var item = MockRepository.Reviews.FirstOrDefault(r => r.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}