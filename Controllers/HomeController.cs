using Microsoft.AspNetCore.Mvc;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System.Linq;

namespace Medication_Tracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var username = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var medications = _context.Medications
                .Where(m => m.UserId == currentUser.Id)
                .ToList();

            var schedules = _context.MedicationSchedules
                .Where(s => s.UserId == currentUser.Id)
                .ToList();

            var logsToday = _context.MedicationLogs
                .Where(l => l.UserId == currentUser.Id && l.DateTaken.Date == DateTime.Today)
                .ToList();

            var now = DateTime.Now.TimeOfDay;

            var upcomingSchedules = schedules
                .Where(s =>
                {
                    if (TimeSpan.TryParse(s.ScheduleTime, out var parsedTime))
                    {
                        return parsedTime >= now;
                    }

                    return false;
                })
                .Where(s => !logsToday.Any(l => l.MedicationScheduleId == s.Id))
                .OrderBy(s => TimeSpan.Parse(s.ScheduleTime))
                .ToList();

            var nextSchedule = upcomingSchedules.FirstOrDefault();

            ViewBag.Medications = medications;
            ViewBag.ScheduleTimes = schedules;
            ViewBag.TotalMedications = medications.Count;
            ViewBag.TakenCount = logsToday.Count(l => l.Status == "Taken");
            ViewBag.MissedCount = logsToday.Count(l => l.Status == "Not Taken");
            ViewBag.RemainingCount = upcomingSchedules.Count;
            ViewBag.NextSchedule = nextSchedule;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogMedication(int medicationId, int scheduleId, string status)
        {
            var username = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Index", "Login");
            }

            var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);

            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var alreadyLogged = _context.MedicationLogs.Any(l =>
                l.UserId == currentUser.Id &&
                l.MedicationScheduleId == scheduleId &&
                l.DateTaken.Date == DateTime.Today);

            if (!alreadyLogged)
            {
                var log = new MedicationLog
                {
                    UserId = currentUser.Id,
                    MedicationId = medicationId,
                    MedicationScheduleId = scheduleId,
                    DateTaken = DateTime.Now,
                    Status = status,
                    Notes = ""
                };

                _context.MedicationLogs.Add(log);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel());
        }
    }
}