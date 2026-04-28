using System.Threading.Tasks;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;  
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Linq;

namespace Medication_Tracker.Controllers
{
    public class MedicationController : Controller
    {
        private readonly ApplicationDbContext context; //database context for accessing the database
        public MedicationController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var meds = context.Medications.ToList();
			return View(meds);
		}
		public IActionResult ViewAll()
        {
            var medications = context.Medications.ToList();
            return View(medications);
        }

        public ActionResult ViewDetails(int id)
        {
            var medication = context.Medications.FirstOrDefault(); // Needs a method that retrieves a specific medication by its name from the database for viewing !! work on this !!
            return View(medication);
        }

        public IActionResult Create()
        {
            return View();
        }
		
		[HttpPost]
        public IActionResult Create(Medication Medication)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Invalid medication data. Please correct the errors and try again.");
			    return View();
			}
            Medication.UserId = 1; 
			context.Medications.Add(Medication);
			context.SaveChanges();
			return RedirectToAction("Index","Medication");
            
      
        }

		public IActionResult Edit(int id)
		{
			var medication = context.Medications.FirstOrDefault(m => m.Id == id);
			if (medication == null)
			{
				return NotFound();
			}
			return View(medication);
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Medication model)
        {

            if (ModelState.IsValid)
            {
                var existingMedication = await context.Medications.FindAsync(model.Id);
                if (existingMedication == null) {
                    return NotFound();
				}
                model.UserId = existingMedication.UserId;
                context.Entry(existingMedication).CurrentValues.SetValues(model);
                await context.SaveChangesAsync();
                return RedirectToAction("Index", "Medication");

            }
            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var medication = context.Medications.FirstOrDefault(m => m.Id == id);
            if (medication == null)
            {
                return NotFound();
			}
			return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var medication = context.Medications.FirstOrDefault(m => m.Id == id);
			if (medication == null)
            {
                return NotFound();
            }
            context.Medications.Remove(medication);
            context.SaveChanges();
            return RedirectToAction("Index", "Medication");

        }
    }
}