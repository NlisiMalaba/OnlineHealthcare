using MediatR;

namespace HealthPlatform.Application.Payments.CreditLine.Notifications;

public sealed record CreditLineChargedNotification(
    Guid CreditLineId,
    Guid PatientId,
    long AmountMinorUnits,
    string Currency,
    long OutstandingBalanceMinorUnits,
    long CreditLimitMinorUnits,
    DateTime ChargedAtUtc,
    DateTime OccurredAtUtc) : INotification;

public sealed record CreditBalanceWarningNotification(
    Guid CreditLineId,
    Guid PatientId,
    long OutstandingBalanceMinorUnits,
    long CreditLimitMinorUnits,
    string Currency,
    DateTime OccurredAtUtc) : INotification;
