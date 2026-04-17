using System;

namespace Medication_Tracker.Models
{
    public class Medication
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty; // e.g., "500mg", "1 tablet"
        public string Frequency { get; set; } = string.Empty; // e.g., "twice daily", "every 8 hours"
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public ICollection<MedicationSchedule> MedicationSchedules { get; set; } = new List<MedicationSchedule>();
        public ICollection<MedicationLog> MedicationLogs { get; set; } = new List<MedicationLog>();

      
	}
}
