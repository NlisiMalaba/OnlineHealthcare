namespace HealthPlatform.Domain.Wellness;

public sealed class InvalidMedicationFrequencyException(string frequency)
    : Exception($"Medication frequency '{frequency}' is not supported for schedule generation.")
{
    public string Frequency { get; } = frequency;
}
