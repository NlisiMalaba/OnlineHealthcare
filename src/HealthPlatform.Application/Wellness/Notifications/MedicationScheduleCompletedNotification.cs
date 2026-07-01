using MediatR;

namespace HealthPlatform.Application.Wellness.Notifications;

public sealed record MedicationScheduleCompletedNotification(
    Guid ScheduleId,
    Guid PrescriptionId,
    Guid PatientId,
    string MedicationName,
    DateTime CompletedAtUtc) : INotification;
