namespace HealthPlatform.Domain.HealthRecords;

public enum HealthRecordEntryType
{
    ConsultationNote = 0,
    Diagnosis = 1,
    PrescriptionRef = 2,
    Allergy = 3,
    Vital = 4,
    LabResultRef = 5,
    Vaccination = 6,
    TelemedicineSessionSummary = 7
}
