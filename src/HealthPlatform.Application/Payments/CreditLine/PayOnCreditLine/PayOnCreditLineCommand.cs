using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Payments.CreditLine.PayOnCreditLine;

public sealed record PayOnCreditLineCommand(
    long AmountMinorUnits,
    string Currency,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId) : ICommand<CreditLinePaymentDto>;
