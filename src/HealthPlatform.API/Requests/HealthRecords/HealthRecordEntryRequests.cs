namespace HealthPlatform.API.Requests.HealthRecords;

public sealed class ConsultationNoteContentRequest
{
    public required string Notes { get; init; }

    public Guid? AppointmentId { get; init; }
}

public sealed class DiagnosisContentRequest
{
    public required IReadOnlyList<string> DiagnosisCodes { get; init; }

    public required string Description { get; init; }
}

public sealed class PrescriptionRefContentRequest
{
    public required Guid PrescriptionId { get; init; }
}

public sealed class AllergyContentRequest
{
    public required string Allergen { get; init; }

    public required string Severity { get; init; }

    public string? Reaction { get; init; }
}

public sealed class VitalContentRequest
{
    public required string VitalType { get; init; }

    public required decimal Value { get; init; }

    public required string Unit { get; init; }

    public required DateTime MeasuredAtUtc { get; init; }
}

public sealed class LabResultRefContentRequest
{
    public required Guid LabResultId { get; init; }
}

public sealed class VaccinationContentRequest
{
    public required string VaccineName { get; init; }

    public required DateTime AdministeredAtUtc { get; init; }

    public string? BatchNumber { get; init; }

    public string? AdministeredBy { get; init; }
}

public sealed class CreateHealthRecordEntryRequest
{
    public required string EntryType { get; init; }

    public bool IsVisibleToPatient { get; init; } = true;

    public ConsultationNoteContentRequest? ConsultationNote { get; init; }

    public DiagnosisContentRequest? Diagnosis { get; init; }

    public PrescriptionRefContentRequest? PrescriptionRef { get; init; }

    public AllergyContentRequest? Allergy { get; init; }

    public VitalContentRequest? Vital { get; init; }

    public LabResultRefContentRequest? LabResultRef { get; init; }

    public VaccinationContentRequest? Vaccination { get; init; }
}

public sealed class UpdateHealthRecordEntryRequest
{
    public bool? IsVisibleToPatient { get; init; }

    public ConsultationNoteContentRequest? ConsultationNote { get; init; }

    public DiagnosisContentRequest? Diagnosis { get; init; }

    public PrescriptionRefContentRequest? PrescriptionRef { get; init; }

    public AllergyContentRequest? Allergy { get; init; }

    public VitalContentRequest? Vital { get; init; }

    public LabResultRefContentRequest? LabResultRef { get; init; }

    public VaccinationContentRequest? Vaccination { get; init; }
}
