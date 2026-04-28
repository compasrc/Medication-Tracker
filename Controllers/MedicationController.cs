using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Medication_Tracker.Data;
using Medication_Tracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Medication_Tracker.Controllers
{
    public class MedicationController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly IHttpClientFactory httpClientFactory;

        public MedicationController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var meds = context.Medications
                .Where(m => m.UserId == userId.Value)
                .ToList();

            var schedules = context.MedicationSchedules
                .Where(s => s.UserId == userId.Value)
                .ToList();

            ViewBag.ScheduleTimes = schedules;
            return View(meds);
        }

        [HttpGet]
        public IActionResult Share()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var medications = context.Medications
                .Where(m => m.UserId == userId.Value)
                .ToList();

            var schedules = context.MedicationSchedules
                .Where(s => s.UserId == userId.Value)
                .ToList();

            ViewBag.ScheduleTimes = schedules;
            return View(medications);
        }

        public IActionResult ViewAll()
        {
            var medications = context.Medications.ToList();
            return View(medications);
        }

        public ActionResult ViewDetails(int id)
        {
            var medication = context.Medications.FirstOrDefault();
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

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            Medication.UserId = userId.Value;
            context.Medications.Add(Medication);
            context.SaveChanges();
            return RedirectToAction("Index", "Medication");
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
                if (existingMedication == null)
                {
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

        [HttpGet]
        public async Task<IActionResult> SideEffects(string? medicationName)
        {
            var model = new SideEffectsViewModel
            {
                MedicationName = medicationName?.Trim() ?? string.Empty
            };

            if (string.IsNullOrWhiteSpace(model.MedicationName))
            {
                return View(model);
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                var safeName = model.MedicationName.Replace("\"", string.Empty);
                var encodedName = Uri.EscapeDataString(safeName);
                var search = $"openfda.brand_name:\"{encodedName}\"+OR+openfda.generic_name:\"{encodedName}\"";
                var url = $"https://api.fda.gov/drug/label.json?search={search}&limit=1";

                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    model.ErrorMessage = "No OpenFDA side effect information was found for that medication.";
                    return View(model);
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                {
                    model.ErrorMessage = "No OpenFDA side effect information was found for that medication.";
                    return View(model);
                }

                var first = results[0];
                model.Alerts = ReadSection(first, "boxed_warning")
                    .Concat(ReadSection(first, "warnings"))
                    .Concat(ReadSection(first, "warnings_and_cautions"))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                model.SideEffects = ReadSection(first, "adverse_reactions")
                    .Concat(ReadSection(first, "adverse_reactions_table"))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                model.Interactions = ReadSection(first, "drug_interactions")
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                model.Contraindications = ReadSection(first, "contraindications")
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                if (!model.Alerts.Any() && !model.SideEffects.Any() && !model.Interactions.Any() && !model.Contraindications.Any())
                {
                    model.ErrorMessage = "OpenFDA returned data, but no side effect sections were available for this medication.";
                }
            }
            catch
            {
                model.ErrorMessage = "Unable to load OpenFDA data right now. Please try again.";
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> SideEffectsSuggestions(string? term)
        {
            var input = term?.Trim();
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            {
                return Json(Array.Empty<string>());
            }

            try
            {
                var client = httpClientFactory.CreateClient();
                var safe = input.Replace("\"", string.Empty);
                var encoded = Uri.EscapeDataString(safe);
                var search = $"openfda.brand_name:{encoded}*+OR+openfda.generic_name:{encoded}*";
                var url = $"https://api.fda.gov/drug/label.json?search={search}&limit=20";

                using var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return Json(Array.Empty<string>());
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var doc = await JsonDocument.ParseAsync(stream);

                if (!doc.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                {
                    return Json(Array.Empty<string>());
                }

                var suggestions = new List<string>();
                foreach (var result in results.EnumerateArray())
                {
                    if (!result.TryGetProperty("openfda", out var openFda))
                    {
                        continue;
                    }

                    AddNames(openFda, "brand_name", suggestions);
                    AddNames(openFda, "generic_name", suggestions);
                }

                var deduped = suggestions
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .Take(10)
                    .ToArray();

                return Json(deduped);
            }
            catch
            {
                return Json(Array.Empty<string>());
            }
        }

        private static List<string> ReadSection(JsonElement item, string propertyName)
        {
            var values = new List<string>();

            if (!item.TryGetProperty(propertyName, out var prop) || prop.ValueKind != JsonValueKind.Array)
            {
                return values;
            }

            foreach (var section in prop.EnumerateArray())
            {
                if (section.ValueKind == JsonValueKind.String)
                {
                    var text = section.GetString();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        values.Add(text.Trim());
                    }
                }
            }

            return values;
        }

        private static void AddNames(JsonElement openFda, string propertyName, List<string> output)
        {
            if (!openFda.TryGetProperty(propertyName, out var names) || names.ValueKind != JsonValueKind.Array)
            {
                return;
            }

            foreach (var name in names.EnumerateArray())
            {
                if (name.ValueKind == JsonValueKind.String)
                {
                    var value = name.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        output.Add(value.Trim());
                    }
                }
            }
        }
    }
}