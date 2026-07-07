namespace HealthPlatform.Application.HealthRecords;

public sealed record ConsultationNoteContent(string Notes, Guid? AppointmentId);

public sealed record DiagnosisContent(IReadOnlyList<string> DiagnosisCodes, string Description);

public sealed record PrescriptionRefContent(Guid PrescriptionId);

public sealed record AllergyContent(string Allergen, string Severity, string? Reaction);

public sealed record VitalContent(string VitalType, decimal Value, string Unit, DateTime MeasuredAtUtc);

public sealed record LabResultRefContent(Guid LabResultId);

public sealed record VaccinationContent(
    string VaccineName,
    DateTime AdministeredAtUtc,
    string? BatchNumber,
    string? AdministeredBy);

public sealed record TelemedicineSessionSummaryContent(
    Guid SessionId,
    Guid AppointmentId,
    Guid DoctorId,
    string SummaryDocumentId);

public sealed record HealthRecordEntryContentPayload(
    ConsultationNoteContent? ConsultationNote = null,
    DiagnosisContent? Diagnosis = null,
    PrescriptionRefContent? PrescriptionRef = null,
    AllergyContent? Allergy = null,
    VitalContent? Vital = null,
    LabResultRefContent? LabResultRef = null,
    VaccinationContent? Vaccination = null,
    TelemedicineSessionSummaryContent? TelemedicineSessionSummary = null);
