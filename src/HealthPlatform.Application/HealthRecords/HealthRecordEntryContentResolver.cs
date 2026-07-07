using HealthPlatform.Application.Exceptions;
using HealthPlatform.Domain.HealthRecords;

namespace HealthPlatform.Application.HealthRecords;

public static class HealthRecordEntryContentResolver
{
    public static HealthRecordEntryContentPayload Resolve(
        HealthRecordEntryType entryType,
        HealthRecordEntryContentPayload payload)
    {
        return entryType switch
        {
            HealthRecordEntryType.ConsultationNote when payload.ConsultationNote is not null =>
                new HealthRecordEntryContentPayload(ConsultationNote: payload.ConsultationNote),
            HealthRecordEntryType.Diagnosis when payload.Diagnosis is not null =>
                new HealthRecordEntryContentPayload(Diagnosis: payload.Diagnosis),
            HealthRecordEntryType.PrescriptionRef when payload.PrescriptionRef is not null =>
                new HealthRecordEntryContentPayload(PrescriptionRef: payload.PrescriptionRef),
            HealthRecordEntryType.Allergy when payload.Allergy is not null =>
                new HealthRecordEntryContentPayload(Allergy: payload.Allergy),
            HealthRecordEntryType.Vital when payload.Vital is not null =>
                new HealthRecordEntryContentPayload(Vital: payload.Vital),
            HealthRecordEntryType.LabResultRef when payload.LabResultRef is not null =>
                new HealthRecordEntryContentPayload(LabResultRef: payload.LabResultRef),
            HealthRecordEntryType.LabOrderRef when payload.LabOrderRef is not null =>
                new HealthRecordEntryContentPayload(LabOrderRef: payload.LabOrderRef),
            HealthRecordEntryType.RadiologyReportRef when payload.RadiologyReportRef is not null =>
                new HealthRecordEntryContentPayload(RadiologyReportRef: payload.RadiologyReportRef),
            HealthRecordEntryType.Vaccination when payload.Vaccination is not null =>
                new HealthRecordEntryContentPayload(Vaccination: payload.Vaccination),
            HealthRecordEntryType.TelemedicineSessionSummary when payload.TelemedicineSessionSummary is not null =>
                new HealthRecordEntryContentPayload(TelemedicineSessionSummary: payload.TelemedicineSessionSummary),
            _ => throw new DomainException(
                HealthRecordErrorCodes.InvalidEntryContent,
                $"Content does not match entry type '{entryType}'.")
        };
    }

    public static HealthRecordEntryContentPayload MergeForUpdate(
        HealthRecordEntryType entryType,
        HealthRecordEntryContentPayload existing,
        HealthRecordEntryContentPayload update)
    {
        var merged = entryType switch
        {
            HealthRecordEntryType.ConsultationNote => new HealthRecordEntryContentPayload(
                ConsultationNote: update.ConsultationNote ?? existing.ConsultationNote),
            HealthRecordEntryType.Diagnosis => new HealthRecordEntryContentPayload(
                Diagnosis: update.Diagnosis ?? existing.Diagnosis),
            HealthRecordEntryType.PrescriptionRef => new HealthRecordEntryContentPayload(
                PrescriptionRef: update.PrescriptionRef ?? existing.PrescriptionRef),
            HealthRecordEntryType.Allergy => new HealthRecordEntryContentPayload(
                Allergy: update.Allergy ?? existing.Allergy),
            HealthRecordEntryType.Vital => new HealthRecordEntryContentPayload(
                Vital: update.Vital ?? existing.Vital),
            HealthRecordEntryType.LabResultRef => new HealthRecordEntryContentPayload(
                LabResultRef: update.LabResultRef ?? existing.LabResultRef),
            HealthRecordEntryType.LabOrderRef => new HealthRecordEntryContentPayload(
                LabOrderRef: update.LabOrderRef ?? existing.LabOrderRef),
            HealthRecordEntryType.RadiologyReportRef => new HealthRecordEntryContentPayload(
                RadiologyReportRef: update.RadiologyReportRef ?? existing.RadiologyReportRef),
            HealthRecordEntryType.Vaccination => new HealthRecordEntryContentPayload(
                Vaccination: update.Vaccination ?? existing.Vaccination),
            HealthRecordEntryType.TelemedicineSessionSummary => new HealthRecordEntryContentPayload(
                TelemedicineSessionSummary: update.TelemedicineSessionSummary ?? existing.TelemedicineSessionSummary),
            _ => throw new DomainException(
                HealthRecordErrorCodes.InvalidEntryContent,
                $"Unsupported entry type '{entryType}'.")
        };

        return Resolve(entryType, merged);
    }
}
