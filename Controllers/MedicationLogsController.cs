using Medication_Tracker.Data;
using Medication_Tracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace Medication_Tracker.Controllers
{
	public class MedicationLogsController : Controller
	{
		private readonly ApplicationDbContext context; //database context for accessing the database

		public MedicationLogsController(ApplicationDbContext context)
		{
			this.context = context;
		}

		public IActionResult Index()
		{
			var logs = context.MedicationLogs.ToList().Where(l => l.TakenAt.HasValue && l.TakenAt.Value.Date == DateTime.Today).ToList();
			return View(logs);

		}
	}
}