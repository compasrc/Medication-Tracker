using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Medication_Tracker.Controllers
{
    public class MedicationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public MedicationController(
            ApplicationDbContext context,
            IMemoryCache cache,
            IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _cache = cache;
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
        public IActionResult Create(Medication medication, string? scheduleTimes, string? Times, string? TimeToTake)
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

            // Accept whichever input name your form is using
            var allTimesText = !string.IsNullOrWhiteSpace(scheduleTimes)
                ? scheduleTimes
                : !string.IsNullOrWhiteSpace(Times)
                    ? Times
                    : TimeToTake;

            if (!string.IsNullOrWhiteSpace(allTimesText))
            {
                var times = allTimesText.Split(',');

                foreach (var time in times)
                {
                    var trimmedTime = time.Trim();

                    if (!string.IsNullOrEmpty(trimmedTime))
                    {
                        _context.MedicationSchedules.Add(new MedicationSchedule
                        {
                            UserId = currentUser.Id,
                            MedicationId = medication.Id,
                            ScheduleTime = trimmedTime,
                            Notes = ""
                        });
                    }
                }

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Interactions()
        {
            return RedirectToAction(nameof(SideEffects));
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

            var normalized = term.Trim().ToLowerInvariant();
            var cacheKey = $"se-suggest:{normalized}";

            if (_cache.TryGetValue(cacheKey, out List<string>? cached) && cached is not null)
                return Json(cached);

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            var token = Uri.EscapeDataString(normalized);
            var urls = new[]
            {
                $"https://api.fda.gov/drug/label.json?search=openfda.brand_name:{token}*&limit=10",
                $"https://api.fda.gov/drug/label.json?search=openfda.generic_name:{token}*&limit=10",
                $"https://api.fda.gov/drug/label.json?search=openfda.substance_name:{token}*&limit=10"
            };

            async Task<List<string>> FetchNames(string url)
            {
                var names = new List<string>();
                try
                {
                    using var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) return names;

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var doc = await JsonDocument.ParseAsync(stream);

                    if (!doc.RootElement.TryGetProperty("results", out var results)) return names;

                    foreach (var result in results.EnumerateArray())
                    {
                        if (!result.TryGetProperty("openfda", out var openfda) || openfda.ValueKind != JsonValueKind.Object)
                            continue;

                        foreach (var prop in new[] { "brand_name", "generic_name", "substance_name" })
                        {
                            if (openfda.TryGetProperty(prop, out var arr) && arr.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var v in arr.EnumerateArray())
                                {
                                    var s = v.GetString();
                                    if (!string.IsNullOrWhiteSpace(s)) names.Add(s.Trim());
                                }
                            }
                        }
                    }
                }
                catch { }
                return names;
            }

            var results = await Task.WhenAll(urls.Select(FetchNames));

            var final = results
                .SelectMany(x => x)
                .Where(n => n.StartsWith(term.Trim(), StringComparison.OrdinalIgnoreCase))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(n => n)
                .Take(25)
                .ToList();

            _cache.Set(cacheKey, final, TimeSpan.FromMinutes(10));
            return Json(final);
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
                var token = Uri.EscapeDataString(term.ToLowerInvariant());

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var urls = new[]
                {
                    $"https://api.fda.gov/drug/label.json?search=openfda.brand_name:{token}*&limit=5",
                    $"https://api.fda.gov/drug/label.json?search=openfda.generic_name:{token}*&limit=5",
                    $"https://api.fda.gov/drug/label.json?search=openfda.substance_name:{token}*&limit=5"
                };

                async Task<bool> TryPopulateFromUrl(string url)
                {
                    using var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) return false;

                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var doc = await JsonDocument.ParseAsync(stream);

                    if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                        return false;

                    foreach (var result in results.EnumerateArray())
                    {
                        void AddArray(JsonElement r, string field, List<string> target)
                        {
                            if (r.TryGetProperty(field, out var arr) && arr.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in arr.EnumerateArray())
                                {
                                    var text = item.GetString();
                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        target.Add(text.Trim());
                                    }
                                }
                            }
                        }

                        AddArray(result, "boxed_warning", vm.Alerts);
                        AddArray(result, "warnings", vm.Alerts);
                        AddArray(result, "warnings_and_cautions", vm.Alerts);

                        AddArray(result, "adverse_reactions", vm.SideEffects);

                        AddArray(result, "drug_interactions", vm.Interactions);

                        AddArray(result, "contraindications", vm.Contraindications);

                        // AddArray(result, "dosage_and_administration", vm.UsageNotes);

                        if (vm.Alerts.Any() || vm.SideEffects.Any() || vm.Interactions.Any() || vm.Contraindications.Any() || vm.UsageNotes.Any())
                            return true;
                    }

                    return false;
                }

                foreach (var url in urls)
                {
                    if (await TryPopulateFromUrl(url))
                        break;
                }

                // Replace final list shaping with this
                vm.Alerts = CleanSection(vm.Alerts, 6);
                vm.SideEffects = CleanSection(vm.SideEffects, 10);
                vm.Interactions = CleanSection(vm.Interactions, 6);
                vm.Contraindications = CleanSection(vm.Contraindications, 6);
                vm.UsageNotes = new List<string>(); // keep hidden for now

                if (!vm.Alerts.Any() && !vm.SideEffects.Any() && !vm.Interactions.Any() && !vm.Contraindications.Any() && !vm.UsageNotes.Any())
                {
                    vm.ErrorMessage = "No structured safety information was available for that medication.";
                }
            }
            catch
            {
                vm.ErrorMessage = "Could not retrieve data from OpenFDA right now.";
            }

            return View(vm);
        }

        private static List<string> CleanSection(IEnumerable<string> source, int maxItems)
        {
            return SplitIntoSentences(source)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(maxItems)
                .ToList();
        }

        private static IEnumerable<string> SplitIntoSentences(IEnumerable<string> source)
        {
            foreach (var block in source.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                var normalized = Regex.Replace(block.Trim(), @"\s+", " ");

                // Split by sentence punctuation or line breaks; keep full sentence text
                var pieces = Regex.Split(normalized, @"(?<=[\.\!\?])\s+|\r?\n+|;\s+");

                foreach (var p in pieces)
                {
                    var sentence = p.Trim();
                    if (!string.IsNullOrWhiteSpace(sentence))
                    {
                        yield return sentence;
                    }
                }
            }
        }
    }
}