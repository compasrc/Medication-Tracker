namespace Medication_Tracker.Models
{
    public class SideEffectsViewModel
    {
        public string MedicationName { get; set; } = string.Empty;

        public List<string> Alerts { get; set; } = new();
        public List<string> SideEffects { get; set; } = new();
        public List<string> Interactions { get; set; } = new();
        public List<string> Contraindications { get; set; } = new();
        public List<string> UsageNotes { get; set; } = new();

        public string? ErrorMessage { get; set; }
    }
}