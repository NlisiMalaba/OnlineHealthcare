using MediatR;

namespace HealthPlatform.Application.Prescriptions.Notifications;

public sealed record PrescriptionCancelledNotification(
    Guid PrescriptionId,
    Guid DoctorId,
    Guid PatientId,
    DateTime CancelledAtUtc,
    DateTime OccurredAtUtc) : INotification;
