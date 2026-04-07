using Microsoft.AspNetCore.Mvc;

namespace Medication_Tracker.Controllers
{
    public class MedicationController : Controller
    {
        public int ID {get; set;}
        public string Description {get; set;} = string.Empty;
        public string Dosage {get; set;} = string.Empty;
        public int Frequency {get; set;}
        public DateTimeOffset DateCreated {get; set;}
        public DateTimeOffset UpdatedAt {get; set;}
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

        public IActionResult Edit()
        {
            return View();
        }
        protected IActionResult Delete()
        {
            return View();
        }
    }
}