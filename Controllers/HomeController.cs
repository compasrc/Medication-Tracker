using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System;
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
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var currentUser = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
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
                .Where(l => l.UserId == currentUser.Id &&
                            l.TakenAt.HasValue &&
                            l.TakenAt.Value.Date == DateTime.Today)
                .ToList();

            var now = DateTime.Now.TimeOfDay;

            var upcomingSchedules = schedules
                .Where(s => TimeSpan.TryParse(s.ScheduleTime, out _))
                .Where(s => !logsToday.Any(l => l.MedicationScheduleId == s.Id))
                .OrderBy(s => TimeSpan.Parse(s.ScheduleTime))
                .ToList();

            var nextSchedule = upcomingSchedules
                .FirstOrDefault(s => TimeSpan.Parse(s.ScheduleTime) >= now)
                ?? upcomingSchedules.FirstOrDefault();

            ViewBag.Medications = medications;
            ViewBag.ScheduleTimes = schedules;
            ViewBag.TodaySchedules = schedules
                .Where(s => TimeSpan.TryParse(s.ScheduleTime, out _))
                .OrderBy(s => TimeSpan.Parse(s.ScheduleTime))
                .ToList();

            ViewBag.LogsToday = logsToday;

            ViewBag.TotalMedications = medications.Count;
            ViewBag.TakenCount = logsToday.Count(l => l.WasTaken);
            ViewBag.MissedCount = logsToday.Count(l => !l.WasTaken);
            ViewBag.RemainingCount = upcomingSchedules.Count;
            ViewBag.NextSchedule = nextSchedule;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogMedication(int medicationId, int scheduleId, string status)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var currentUser = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var existingLog = _context.MedicationLogs
                .FirstOrDefault(l =>
                    l.UserId == currentUser.Id &&
                    l.MedicationScheduleId == scheduleId &&
                    l.TakenAt.HasValue &&
                    l.TakenAt.Value.Date == DateTime.Today);

            if (existingLog != null)
            {
                existingLog.Status = status;
                existingLog.TakenAt = DateTime.Now;
                existingLog.MedicationId = medicationId;
                existingLog.WasTaken = status == "Taken";
                existingLog.CreatedAt = DateTime.Now;
            }
            else
            {
                var log = new MedicationLog
                {
                    UserId = currentUser.Id,
                    MedicationId = medicationId,
                    MedicationScheduleId = scheduleId,
                    TakenAt = DateTime.Now,
                    CreatedAt = DateTime.Now,
                    WasTaken = status == "Taken",
                    Status = status,
                    Notes = ""
                };

                _context.MedicationLogs.Add(log);
            }

            _context.SaveChanges();

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