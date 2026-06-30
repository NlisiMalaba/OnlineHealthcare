using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Payments.Instalments.Events;

public sealed record InstalmentPlanCreatedDomainEvent(
    Guid InstalmentPlanId,
    Guid PatientId,
    long TotalAmountMinorUnits,
    string Currency,
    int TotalInstalments) : DomainEvent;

public sealed record InstalmentPaymentMissedDomainEvent(
    Guid InstalmentPaymentId,
    Guid InstalmentPlanId,
    Guid PatientId,
    int SequenceNumber,
    long AmountMinorUnits,
    long LateFeeMinorUnits,
    string Currency,
    DateOnly DueDate) : DomainEvent;
