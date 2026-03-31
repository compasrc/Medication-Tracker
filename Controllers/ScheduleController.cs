using Microsoft.AspNetCore.Mvc;
namespace Medication_Tracker.Controllers
{
    public class ScheduleController : Controller
    {
        public int ID {get; set;}
        public int UserID {get; set;}
        public int MedicationID {get; set;}
        public string ScheduleTime {get; set;} = string.Empty;
        public DateTime StartDate {get; set;}
        public DateTime EndDate {get; set;}
        public bool IsActive {get; set;}

        public DateTime CreatedAt {get; set;}
        public DateTime UpdatedAt {get; set;}
        
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