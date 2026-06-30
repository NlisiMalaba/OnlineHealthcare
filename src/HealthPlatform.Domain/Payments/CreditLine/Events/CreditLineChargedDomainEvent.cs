using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Payments.CreditLine.Events;

public sealed record CreditLineChargedDomainEvent(
    Guid CreditLineId,
    Guid PatientId,
    long AmountMinorUnits,
    string Currency,
    long OutstandingBalanceMinorUnits,
    long CreditLimitMinorUnits,
    DateTime ChargedAtUtc) : DomainEvent;
