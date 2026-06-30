using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments.Instalments;

public sealed record InstalmentScheduleItemDto(
    int SequenceNumber,
    long AmountMinorUnits,
    DateOnly DueDate);

public sealed record InstalmentPlanPreviewDto(
    long TotalAmountMinorUnits,
    long InstalmentAmountMinorUnits,
    long TotalRepayableMinorUnits,
    InstalmentFrequency Frequency,
    int TotalInstalments,
    string Currency,
    DateOnly FirstDueDate,
    IReadOnlyList<InstalmentScheduleItemDto> Schedule);

public sealed record InstalmentPlanDto(
    Guid Id,
    long TotalAmountMinorUnits,
    long InstalmentAmountMinorUnits,
    long TotalRepayableMinorUnits,
    InstalmentFrequency Frequency,
    int TotalInstalments,
    int PaidInstalments,
    InstalmentPlanStatus Status,
    string Currency,
    DateOnly FirstDueDate,
    IReadOnlyList<InstalmentScheduleItemDto> Schedule);

public sealed record InstalmentPlanListItemDto(
    Guid Id,
    long TotalAmountMinorUnits,
    long TotalRepayableMinorUnits,
    InstalmentFrequency Frequency,
    int TotalInstalments,
    int PaidInstalments,
    InstalmentPlanStatus Status,
    string Currency,
    DateOnly FirstDueDate);
