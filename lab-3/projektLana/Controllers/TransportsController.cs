using Microsoft.AspNetCore.Mvc;
using projektLana.Data;

namespace projektLana.Controllers
{
    public class TransportsController : Controller
    {
        public IActionResult Index() => View(MockRepository.Transports);

        public IActionResult Details(int id)
        {
            var item = MockRepository.Transports.FirstOrDefault(t => t.Id == id);
            if (item == null) return NotFound();
            return View(item);
        }
    }
}