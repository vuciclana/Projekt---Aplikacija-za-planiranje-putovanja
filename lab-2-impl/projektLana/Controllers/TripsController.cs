using Microsoft.AspNetCore.Mvc;
using projektLana.Data;
using System.Linq;

namespace projektLana.Controllers
{
    public class TripsController : Controller
    {
        public IActionResult Index() => View(MockRepository.Trips);
        
        public IActionResult Details(int id)
        {
            var trip = MockRepository.Trips.FirstOrDefault(t => t.Id == id);
            if (trip == null) return NotFound();
            return View(trip);
        }
    }
}