using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Payments.Instalments;

namespace HealthPlatform.Application.Payments.Instalments.CreateInstalmentPlan;

public sealed record CreateInstalmentPlanCommand(
    long TotalAmountMinorUnits,
    InstalmentFrequency Frequency,
    int InstalmentCount,
    string Currency,
    DateOnly FirstDueDate,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId) : ICommand<InstalmentPlanDto>;
