using ApiRequests = HealthPlatform.API.Requests.HealthRecords;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.CreateHealthRecordEntry;
using HealthPlatform.Application.HealthRecords.UpdateHealthRecordEntry;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.API.Mapping;

public static class HealthRecordEntryCommandMapper
{
    public static CreateHealthRecordEntryCommand ToCreateCommand(
        Guid healthRecordId,
        ApiRequests.CreateHealthRecordEntryRequest request) =>
        new(
            healthRecordId,
            ParseEntryType(request.EntryType),
            ToContentPayload(request),
            request.IsVisibleToPatient);

    public static UpdateHealthRecordEntryCommand ToUpdateCommand(
        string entryId,
        ApiRequests.UpdateHealthRecordEntryRequest request) =>
        new(
            entryId,
            ToContentPayload(request),
            request.IsVisibleToPatient);

    private static HealthRecordEntryContentPayload ToContentPayload(ApiRequests.CreateHealthRecordEntryRequest request) =>
        new(
            ConsultationNote: request.ConsultationNote is null
                ? null
                : new ConsultationNoteContent(request.ConsultationNote.Notes, request.ConsultationNote.AppointmentId),
            Diagnosis: request.Diagnosis is null
                ? null
                : new DiagnosisContent(request.Diagnosis.DiagnosisCodes, request.Diagnosis.Description),
            PrescriptionRef: request.PrescriptionRef is null
                ? null
                : new PrescriptionRefContent(request.PrescriptionRef.PrescriptionId),
            Allergy: request.Allergy is null
                ? null
                : new AllergyContent(
                    request.Allergy.Allergen,
                    request.Allergy.Severity,
                    request.Allergy.Reaction),
            Vital: request.Vital is null
                ? null
                : new VitalContent(
                    request.Vital.VitalType,
                    request.Vital.Value,
                    request.Vital.Unit,
                    request.Vital.MeasuredAtUtc),
            LabResultRef: request.LabResultRef is null
                ? null
                : new LabResultRefContent(request.LabResultRef.LabResultId),
            Vaccination: request.Vaccination is null
                ? null
                : new VaccinationContent(
                    request.Vaccination.VaccineName,
                    request.Vaccination.AdministeredAtUtc,
                    request.Vaccination.BatchNumber,
                    request.Vaccination.AdministeredBy));

    private static HealthRecordEntryContentPayload ToContentPayload(ApiRequests.UpdateHealthRecordEntryRequest request) =>
        new(
            ConsultationNote: request.ConsultationNote is null
                ? null
                : new ConsultationNoteContent(request.ConsultationNote.Notes, request.ConsultationNote.AppointmentId),
            Diagnosis: request.Diagnosis is null
                ? null
                : new DiagnosisContent(request.Diagnosis.DiagnosisCodes, request.Diagnosis.Description),
            PrescriptionRef: request.PrescriptionRef is null
                ? null
                : new PrescriptionRefContent(request.PrescriptionRef.PrescriptionId),
            Allergy: request.Allergy is null
                ? null
                : new AllergyContent(
                    request.Allergy.Allergen,
                    request.Allergy.Severity,
                    request.Allergy.Reaction),
            Vital: request.Vital is null
                ? null
                : new VitalContent(
                    request.Vital.VitalType,
                    request.Vital.Value,
                    request.Vital.Unit,
                    request.Vital.MeasuredAtUtc),
            LabResultRef: request.LabResultRef is null
                ? null
                : new LabResultRefContent(request.LabResultRef.LabResultId),
            Vaccination: request.Vaccination is null
                ? null
                : new VaccinationContent(
                    request.Vaccination.VaccineName,
                    request.Vaccination.AdministeredAtUtc,
                    request.Vaccination.BatchNumber,
                    request.Vaccination.AdministeredBy));

    private static HealthRecordEntryType ParseEntryType(string entryType) =>
        entryType.Trim().ToLowerInvariant() switch
        {
            "consultation_note" => HealthRecordEntryType.ConsultationNote,
            "diagnosis" => HealthRecordEntryType.Diagnosis,
            "prescription_ref" => HealthRecordEntryType.PrescriptionRef,
            "allergy" => HealthRecordEntryType.Allergy,
            "vital" => HealthRecordEntryType.Vital,
            "lab_result_ref" => HealthRecordEntryType.LabResultRef,
            "radiology_report_ref" => HealthRecordEntryType.RadiologyReportRef,
            "lab_order_ref" => HealthRecordEntryType.LabOrderRef,
            "diagnostic_report_annotation" => HealthRecordEntryType.DiagnosticReportAnnotation,
            "vaccination" => HealthRecordEntryType.Vaccination,
            _ => throw new ArgumentException($"Unsupported entry type '{entryType}'.", nameof(entryType))
        };
}
