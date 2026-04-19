using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System.Linq;

namespace Medication_Tracker.Controllers
{
    public class MedicationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicationController(ApplicationDbContext context)
        {
            _context = context;
        }

        private User? GetCurrentUser()
        {
            var username = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public IActionResult Index()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var medications = _context.Medications
                .Where(m => m.UserId == currentUser.Id)
                .ToList();

            
            ViewBag.ScheduleTimes = _context.MedicationSchedules
                .Where(s => s.UserId == currentUser.Id)
                .ToList();

            return View(medications);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Medication medication, string scheduleTimes)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (!ModelState.IsValid)
            {
                return View(medication);
            }

            medication.UserId = currentUser.Id;
            medication.CreatedAt = DateTime.UtcNow;

            _context.Medications.Add(medication);
            _context.SaveChanges();

            if (!string.IsNullOrWhiteSpace(scheduleTimes))
            {
                var times = scheduleTimes.Split(',');

                foreach (var time in times)
                {
                    var trimmedTime = time.Trim();

                    if (!string.IsNullOrEmpty(trimmedTime))
                    {
                        var schedule = new MedicationSchedule
                        {
                            UserId = currentUser.Id,
                            MedicationId = medication.Id,
                            ScheduleTime = trimmedTime,
                            Notes = ""
                        };

                        _context.MedicationSchedules.Add(schedule);
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Interactions()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var medication = _context.Medications
                .FirstOrDefault(m => m.Id == id && m.UserId == currentUser.Id);

            if (medication == null)
            {
                return NotFound();
            }

            var scheduleTimes = _context.MedicationSchedules
                .Where(s => s.MedicationId == medication.Id && s.UserId == currentUser.Id)
                .Select(s => s.ScheduleTime)
                .ToList();

            ViewBag.ScheduleTimes = string.Join(", ", scheduleTimes);

            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Medication medication, string scheduleTimes)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (id != medication.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.ScheduleTimes = scheduleTimes;
                return View(medication);
            }

            var existingMedication = _context.Medications
                .FirstOrDefault(m => m.Id == id && m.UserId == currentUser.Id);

            if (existingMedication == null)
            {
                return NotFound();
            }

            existingMedication.Name = medication.Name;
            existingMedication.Dosage = medication.Dosage;
            existingMedication.Frequency = medication.Frequency;
            existingMedication.Description = medication.Description;
            existingMedication.UpdatedAt = DateTime.UtcNow;

            var oldSchedules = _context.MedicationSchedules
                .Where(s => s.MedicationId == existingMedication.Id && s.UserId == currentUser.Id)
                .ToList();

            _context.MedicationSchedules.RemoveRange(oldSchedules);

            if (!string.IsNullOrWhiteSpace(scheduleTimes))
            {
                var times = scheduleTimes.Split(',');

                foreach (var time in times)
                {
                    var trimmedTime = time.Trim();

                    if (!string.IsNullOrEmpty(trimmedTime))
                    {
                        _context.MedicationSchedules.Add(new MedicationSchedule
                        {
                            UserId = currentUser.Id,
                            MedicationId = existingMedication.Id,
                            ScheduleTime = trimmedTime,
                            Notes = ""
                        });
                    }
                }
            }

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var medication = _context.Medications
                .FirstOrDefault(m => m.Id == id && m.UserId == currentUser.Id);

            if (medication == null)
            {
                return NotFound();
            }

            return View(medication);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var medication = _context.Medications
                .FirstOrDefault(m => m.Id == id && m.UserId == currentUser.Id);

            if (medication == null)
            {
                return NotFound();
            }

            _context.Medications.Remove(medication);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}