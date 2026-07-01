namespace HealthPlatform.Application.Wellness;

public interface IMedicationScheduleCompletionNotifier
{
    Task NotifyScheduleCompletedAsync(
        MedicationScheduleCompletionNotice notice,
        CancellationToken ct);
}

public sealed record MedicationScheduleCompletionNotice(
    Guid ScheduleId,
    Guid PrescriptionId,
    Guid PatientId,
    Guid PatientUserId,
    Guid DoctorId,
    Guid DoctorUserId,
    string MedicationName,
    DateTime CompletedAtUtc);
