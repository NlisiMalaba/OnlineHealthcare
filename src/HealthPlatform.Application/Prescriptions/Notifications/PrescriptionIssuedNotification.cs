using MediatR;

namespace HealthPlatform.Application.Prescriptions.Notifications;

public sealed record PrescriptionIssuedNotification(
    Guid PrescriptionId,
    Guid DoctorId,
    Guid PatientId,
    Guid HealthRecordId,
    DateTime IssuedAtUtc,
    DateTime ExpiresAtUtc,
    DateTime OccurredAtUtc) : INotification;
