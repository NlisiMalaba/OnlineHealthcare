using HealthPlatform.Domain.Common;

namespace HealthPlatform.Domain.Payments.CreditLine;

public sealed class CreditLineTransaction : Entity
{
    private CreditLineTransaction()
    {
        Currency = string.Empty;
    }

    public Guid CreditLineId { get; private set; }

    public Guid PatientId { get; private set; }

    public CreditLineTransactionType TransactionType { get; private set; }

    public long AmountMinorUnits { get; private set; }

    public string Currency { get; private set; }

    public long OutstandingBalanceAfterMinorUnits { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public Guid? MedicationOrderId { get; private set; }

    public Guid? LabOrderId { get; private set; }

    public DateTime RepaymentDueAtUtc { get; private set; }

    public bool RepaymentReminderSent { get; private set; }

    public static CreditLineTransaction RecordCharge(
        Guid creditLineId,
        Guid patientId,
        long amountMinorUnits,
        string currency,
        long outstandingBalanceAfterMinorUnits,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId,
        DateTime chargedAtUtc,
        DateTime repaymentDueAtUtc)
    {
        ValidateTarget(appointmentId, medicationOrderId, labOrderId);

        return new CreditLineTransaction
        {
            CreditLineId = creditLineId,
            PatientId = patientId,
            TransactionType = CreditLineTransactionType.Charge,
            AmountMinorUnits = amountMinorUnits,
            Currency = currency.Trim().ToUpperInvariant(),
            OutstandingBalanceAfterMinorUnits = outstandingBalanceAfterMinorUnits,
            AppointmentId = appointmentId,
            MedicationOrderId = medicationOrderId,
            LabOrderId = labOrderId,
            RepaymentDueAtUtc = repaymentDueAtUtc,
            RepaymentReminderSent = false,
            CreatedAtUtc = chargedAtUtc,
            UpdatedAtUtc = chargedAtUtc
        };
    }

    public void MarkRepaymentReminderSent()
    {
        RepaymentReminderSent = true;
        Touch();
    }

    private static void ValidateTarget(Guid? appointmentId, Guid? medicationOrderId, Guid? labOrderId)
    {
        var targetCount = new[] { appointmentId, medicationOrderId, labOrderId }.Count(id => id is not null);
        if (targetCount != 1)
        {
            throw new ArgumentException("Exactly one charge target id is required.");
        }
    }
}
