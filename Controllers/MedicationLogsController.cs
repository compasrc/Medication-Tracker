using Microsoft.AspNetCore.Mvc;
namespace Medication_Tracker.Controllers
{
    public class MedicationLogsController : Controller
    {
        public int ID {get; set;}
        public int UserID {get; set;}
        public int MedicationID {get; set;}
        public int MedicationScheduleID {get; set;}
        public DateTime TakenAt {get; set;}
        public bool WasTaken {get; set;}
        public string Notes {get; set;} = string.Empty;
        public DateTime CreatedAt {get; set;}
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
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