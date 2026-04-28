using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Medication_Tracker.Models
{
    public class Medication
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ValidateNever]
        public User User { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Dosage { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string? TimeToTake { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [ValidateNever]
        public ICollection<MedicationSchedule> MedicationSchedules { get; set; } = new List<MedicationSchedule>();

        [ValidateNever]
        public ICollection<MedicationLog> MedicationLogs { get; set; } = new List<MedicationLog>();

      
	}
}
