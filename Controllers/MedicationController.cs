using Microsoft.AspNetCore.Mvc;

namespace Medication_Tracker.Controllers
{
    public class MedicationController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            return View();
        }

        public IActionResult Interactions()
        {
            return View();
        }
    }
}