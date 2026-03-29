using System;

namespace Medication_Tracker.Models
{
    public class MedicationLog
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MedicationId { get; set; }
        public int MedicationScheduleId { get; set; }
        public DateTime TakenAt { get; set; }
        public bool WasTaken { get; set; } = true;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Medication? Medication { get; set; }
        public MedicationSchedule? MedicationSchedule { get; set; }
    }
}
