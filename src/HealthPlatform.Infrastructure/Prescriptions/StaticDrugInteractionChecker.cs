using HealthPlatform.Application.Prescriptions.DrugInteractions;

namespace HealthPlatform.Infrastructure.Prescriptions;

public sealed class StaticDrugInteractionChecker : IDrugInteractionChecker
{
    private static readonly IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> InteractionRules =
        new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["warfarin"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ibuprofen"] = "Increased bleeding risk when combined with warfarin.",
                ["aspirin"] = "Increased bleeding risk when combined with warfarin.",
                ["naproxen"] = "Increased bleeding risk when combined with warfarin."
            },
            ["ibuprofen"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["warfarin"] = "Increased bleeding risk when combined with ibuprofen.",
                ["aspirin"] = "Increased gastrointestinal bleeding risk when combined with ibuprofen.",
                ["lisinopril"] = "NSAIDs may reduce antihypertensive efficacy and increase renal risk."
            },
            ["metformin"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["contrast dye"] = "Risk of lactic acidosis when contrast media is used with metformin."
            },
            ["simvastatin"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["amiodarone"] = "Increased risk of myopathy and rhabdomyolysis."
            },
            ["fluoxetine"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["phenelzine"] = "Risk of serotonin syndrome with MAOI combination."
            }
        };

    public IReadOnlyList<DrugInteractionAlert> Check(
        string proposedMedicationName,
        IReadOnlyList<string> activeMedicationNames)
    {
        if (string.IsNullOrWhiteSpace(proposedMedicationName) || activeMedicationNames.Count == 0)
        {
            return [];
        }

        var normalizedProposed = Normalize(proposedMedicationName);
        if (!InteractionRules.TryGetValue(normalizedProposed, out var proposedInteractions))
        {
            return [];
        }

        var alerts = new List<DrugInteractionAlert>();
        foreach (var activeMedication in activeMedicationNames)
        {
            if (string.IsNullOrWhiteSpace(activeMedication))
            {
                continue;
            }

            var normalizedActive = Normalize(activeMedication);
            if (proposedInteractions.TryGetValue(normalizedActive, out var description))
            {
                alerts.Add(new DrugInteractionAlert(activeMedication.Trim(), description));
            }
        }

        return alerts;
    }

    private static string Normalize(string medicationName) => medicationName.Trim();
}
