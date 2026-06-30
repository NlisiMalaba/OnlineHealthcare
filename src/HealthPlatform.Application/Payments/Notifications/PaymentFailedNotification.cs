using MediatR;

namespace HealthPlatform.Application.Payments.Notifications;

public sealed record PaymentFailedNotification(
    Guid PaymentId,
    Guid PatientId,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId,
    string FailureCode,
    string FailureMessage,
    DateTime RetentionExpiresAtUtc,
    DateTime OccurredAtUtc) : INotification;
