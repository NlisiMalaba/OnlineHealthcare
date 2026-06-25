namespace HealthPlatform.Application.Prescriptions.DrugInteractions;

public interface IDrugInteractionChecker
{
    IReadOnlyList<DrugInteractionAlert> Check(
        string proposedMedicationName,
        IReadOnlyList<string> activeMedicationNames);
}
