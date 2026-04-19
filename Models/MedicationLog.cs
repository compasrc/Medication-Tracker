using System;

namespace Medication_Tracker.Models
{
    public class MedicationLog
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public int MedicationId { get; set; }
        public Medication Medication { get; set; } = null!;

        public int MedicationScheduleId { get; set; }
        public MedicationSchedule MedicationSchedule { get; set; } = null!;

        public DateTime DateTaken { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}