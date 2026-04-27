using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
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
                return null;

            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

       
        public IActionResult Index()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            var medications = _context.Medications
                .Where(m => m.UserId == currentUser.Id)
                .ToList();

            var schedules = _context.MedicationSchedules
                .Where(s => s.UserId == currentUser.Id)
                .ToList();

            ViewBag.ScheduleTimes = schedules;

            return View(medications);
        }


        public IActionResult Create()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Medication medication, string times)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            medication.UserId = currentUser.Id;

            _context.Medications.Add(medication);
            _context.SaveChanges();


            if (!string.IsNullOrEmpty(times))
            {
                var timeList = times.Split(',');

                foreach (var time in timeList)
                {
                    var trimmed = time.Trim();

                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        _context.MedicationSchedules.Add(new MedicationSchedule
                        {
                            MedicationId = medication.Id,
                            UserId = currentUser.Id,
                            ScheduleTime = trimmed
                        });
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

       
        public IActionResult Edit(int id)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            var medication = _context.Medications
                .FirstOrDefault(m => m.Id == id && m.UserId == currentUser.Id);

            if (medication == null)
                return NotFound();

            return View(medication);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Medication medication)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            medication.UserId = currentUser.Id;

            _context.Medications.Update(medication);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

       
        public IActionResult Delete(int id)
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            var medication = _context.Medications
                .FirstOrDefault(m => m.Id == id && m.UserId == currentUser.Id);

            if (medication != null)
            {
                // delete schedules first
                var schedules = _context.MedicationSchedules
                    .Where(s => s.MedicationId == medication.Id)
                    .ToList();

                _context.MedicationSchedules.RemoveRange(schedules);

                _context.Medications.Remove(medication);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        
        public IActionResult Share()
        {
            var currentUser = GetCurrentUser();

            if (currentUser == null)
                return RedirectToAction("Index", "Login");

            var medications = _context.Medications
                .Where(m => m.UserId == currentUser.Id)
                .ToList();

            var schedules = _context.MedicationSchedules
                .Where(s => s.UserId == currentUser.Id)
                .ToList();

            ViewBag.ScheduleTimes = schedules;

            return View(medications);
        }
    }
}