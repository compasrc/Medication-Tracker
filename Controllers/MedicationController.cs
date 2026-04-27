using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System.Linq;
using System.Text.Json;
using System.Net;

namespace Medication_Tracker.Controllers
{
    public class MedicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public MedicationController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
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
        public IActionResult Create(Medication medication, string? scheduleTimes)
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
            // keep old route working
            return RedirectToAction(nameof(SideEffects));
        }

        [HttpGet]
        public async Task<IActionResult> SideEffects(string? medicationName)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var vm = new SideEffectsViewModel
            {
                MedicationName = medicationName ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(medicationName))
            {
                return View(vm);
            }

            try
            {
                var term = medicationName.Trim();
                var encodedTerm = WebUtility.UrlEncode(term);

                // Ask OpenFDA for records that match name and have side-effect-related content
                var url =
                    "https://api.fda.gov/drug/label.json" +
                    $"?search=((openfda.brand_name:\"{encodedTerm}\"+OR+openfda.generic_name:\"{encodedTerm}\"+OR+openfda.substance_name:\"{encodedTerm}\")" +
                    "+AND+(_exists_:adverse_reactions+OR+_exists_:warnings+OR+_exists_:warnings_and_cautions))" +
                    "&limit=10";

                var client = _httpClientFactory.CreateClient();
                using var response = await client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    vm.ErrorMessage = "No side effect data found for that medication.";
                    return View(vm);
                }

                using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                {
                    vm.ErrorMessage = "No side effect data found for that medication.";
                    return View(vm);
                }

                foreach (var result in results.EnumerateArray())
                {
                    // Preferred field
                    if (result.TryGetProperty("adverse_reactions", out var adverse) && adverse.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in adverse.EnumerateArray())
                        {
                            var text = item.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                vm.SideEffects.Add(text.Trim());
                            }
                        }
                    }

                    // Fallback fields
                    if (vm.SideEffects.Count == 0 &&
                        result.TryGetProperty("warnings_and_cautions", out var cautions) &&
                        cautions.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in cautions.EnumerateArray())
                        {
                            var text = item.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                vm.SideEffects.Add(text.Trim());
                            }
                        }
                    }

                    if (vm.SideEffects.Count == 0 &&
                        result.TryGetProperty("warnings", out var warnings) &&
                        warnings.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in warnings.EnumerateArray())
                        {
                            var text = item.GetString();
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                vm.SideEffects.Add(text.Trim());
                            }
                        }
                    }

                    if (vm.SideEffects.Count > 0)
                    {
                        break; // stop when we found usable content
                    }
                }

                if (vm.SideEffects.Count == 0)
                {
                    vm.ErrorMessage = "No side effects section was available for that medication.";
                }
            }
            catch
            {
                vm.ErrorMessage = "Could not retrieve data from OpenFDA right now.";
            }

            return View(vm);
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

        [HttpGet]
        public async Task<IActionResult> SideEffectsSuggestions(string? term)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(term) || term.Trim().Length < 3)
                return Json(new List<string>());

            var token = Uri.EscapeDataString(term.Trim().ToLowerInvariant());
            var client = _httpClientFactory.CreateClient();

            var queries = new[]
            {
                $"openfda.brand_name:{token}*",
                $"openfda.generic_name:{token}*",
                $"openfda.substance_name:{token}*"
            };

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var q in queries)
            {
                try
                {
                    var url = $"https://api.fda.gov/drug/label.json?search={q}&limit=15";
                    using var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var doc = await JsonDocument.ParseAsync(stream);

                    if (!doc.RootElement.TryGetProperty("results", out var results)) continue;

                    foreach (var result in results.EnumerateArray())
                    {
                        if (!result.TryGetProperty("openfda", out var openfda) || openfda.ValueKind != JsonValueKind.Object)
                            continue;

                        void Add(string prop)
                        {
                            if (openfda.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var v in arr.EnumerateArray())
                                {
                                    var s = v.GetString();
                                    if (!string.IsNullOrWhiteSpace(s))
                                        names.Add(s.Trim());
                                }
                            }
                        }

                        Add("brand_name");
                        Add("generic_name");
                        Add("substance_name");
                    }
                }
                catch
                {
                    // ignore failed query and continue
                }
            }

            return Json(names
                .Where(n => n.StartsWith(term.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n)
                .Take(25)
                .ToList());
        }
    }
}