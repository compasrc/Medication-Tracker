namespace Medication_Tracker.Models
{
    public class SideEffectsViewModel
    {
        public string MedicationName { get; set; } = string.Empty;
        public List<string> SideEffects { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}