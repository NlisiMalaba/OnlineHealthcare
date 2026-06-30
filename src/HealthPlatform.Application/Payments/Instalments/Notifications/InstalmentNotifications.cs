using MediatR;

namespace HealthPlatform.Application.Payments.Instalments.Notifications;

public sealed record InstalmentPaymentMissedNotification(
    Guid InstalmentPaymentId,
    Guid InstalmentPlanId,
    Guid PatientId,
    int SequenceNumber,
    long AmountMinorUnits,
    long LateFeeMinorUnits,
    string Currency,
    DateOnly DueDate,
    DateTime OccurredAtUtc) : INotification;
