namespace HealthPlatform.Application.Wellness;

public sealed record DueMedicationDose(
    Guid ScheduleId,
    Guid PatientId,
    string MedicationName,
    DateTime ScheduledAtUtc);
