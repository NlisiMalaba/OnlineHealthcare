namespace HealthPlatform.Application.Prescriptions.DrugInteractions;

public sealed record DrugInteractionAlert(
    string InteractingMedicationName,
    string Description);
