using System.Globalization;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public static class HealthRecordEntryFormatter
{
    public static string FormatEntryType(HealthRecordEntryType entryType) =>
        entryType switch
        {
            HealthRecordEntryType.ConsultationNote => "Consultation note",
            HealthRecordEntryType.Diagnosis => "Diagnosis",
            HealthRecordEntryType.PrescriptionRef => "Prescription reference",
            HealthRecordEntryType.Allergy => "Allergy",
            HealthRecordEntryType.Vital => "Vital sign",
            HealthRecordEntryType.LabResultRef => "Lab result reference",
            HealthRecordEntryType.LabOrderRef => "Lab order reference",
            HealthRecordEntryType.RadiologyReportRef => "Radiology report reference",
            HealthRecordEntryType.Vaccination => "Vaccination",
            HealthRecordEntryType.TelemedicineSessionSummary => "Telemedicine session summary",
            _ => entryType.ToString()
        };

    public static IReadOnlyList<string> FormatEntryLines(HealthRecordEntryDto entry)
    {
        var lines = new List<string>
        {
            $"Recorded: {entry.CreatedAtUtc:u}"
        };

        if (entry.Content.ConsultationNote is { } note)
        {
            lines.Add($"Notes: {note.Notes}");
            if (note.AppointmentId is { } appointmentId)
            {
                lines.Add($"Appointment: {appointmentId}");
            }
        }

        if (entry.Content.Diagnosis is { } diagnosis)
        {
            lines.Add($"Description: {diagnosis.Description}");
            lines.Add($"Codes: {string.Join(", ", diagnosis.DiagnosisCodes)}");
        }

        if (entry.Content.PrescriptionRef is { } prescriptionRef)
        {
            lines.Add($"Prescription: {prescriptionRef.PrescriptionId}");
        }

        if (entry.Content.Allergy is { } allergy)
        {
            lines.Add($"Allergen: {allergy.Allergen}");
            lines.Add($"Severity: {allergy.Severity}");
            if (!string.IsNullOrWhiteSpace(allergy.Reaction))
            {
                lines.Add($"Reaction: {allergy.Reaction}");
            }
        }

        if (entry.Content.Vital is { } vital)
        {
            lines.Add(
                string.Create(
                    CultureInfo.InvariantCulture,
                    $"{vital.VitalType}: {vital.Value} {vital.Unit} at {vital.MeasuredAtUtc:u}"));
        }

        if (entry.Content.LabResultRef is { } labResultRef)
        {
            lines.Add($"Lab result: {labResultRef.LabResultId}");
        }

        if (entry.Content.LabOrderRef is { } labOrderRef)
        {
            lines.Add($"Lab order: {labOrderRef.LabOrderId}");
            lines.Add($"Test code: {labOrderRef.TestCode}");
            lines.Add($"Lab partner: {labOrderRef.LabPartnerCode}");
        }

        if (entry.Content.RadiologyReportRef is { } radiologyRef)
        {
            lines.Add($"Radiology report: {radiologyRef.RadiologyReportId}");
        }

        if (entry.Content.Vaccination is { } vaccination)
        {
            lines.Add($"Vaccine: {vaccination.VaccineName}");
            lines.Add($"Administered: {vaccination.AdministeredAtUtc:u}");
            if (!string.IsNullOrWhiteSpace(vaccination.BatchNumber))
            {
                lines.Add($"Batch: {vaccination.BatchNumber}");
            }

            if (!string.IsNullOrWhiteSpace(vaccination.AdministeredBy))
            {
                lines.Add($"Administered by: {vaccination.AdministeredBy}");
            }
        }

        if (entry.Content.TelemedicineSessionSummary is { } summary)
        {
            lines.Add($"Session: {summary.SessionId}");
            lines.Add($"Appointment: {summary.AppointmentId}");
            lines.Add($"Summary document: {summary.SummaryDocumentId}");
        }

        return lines;
    }
}
