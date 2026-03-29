using System;

namespace Medication_Tracker.Models
{
    public class MedicationSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MedicationId { get; set; }
        public string ScheduleTime { get; set; } = string.Empty; // e.g., "08:00 AM", "02:00 PM"
        public string? Notes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public User? User { get; set; }
        public Medication? Medication { get; set; }
        public ICollection<MedicationLog> MedicationLogs { get; set; } = new List<MedicationLog>();
    }
}
