using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Payments.CreditLine.Events;

public sealed record CreditBalanceWarningTriggeredDomainEvent(
    Guid CreditLineId,
    Guid PatientId,
    long OutstandingBalanceMinorUnits,
    long CreditLimitMinorUnits,
    string Currency) : DomainEvent;
