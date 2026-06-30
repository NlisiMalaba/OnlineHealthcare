using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Payments.Instalments.Events;

namespace HealthPlatform.Domain.Payments.Instalments;

public sealed class InstalmentPayment : Entity
{
    private InstalmentPayment()
    {
        Currency = string.Empty;
    }

    public Guid InstalmentPlanId { get; private set; }

    public Guid PatientId { get; private set; }

    public int SequenceNumber { get; private set; }

    public long AmountMinorUnits { get; private set; }

    public long LateFeeMinorUnits { get; private set; }

    public string Currency { get; private set; }

    public DateOnly DueDate { get; private set; }

    public InstalmentPaymentStatus Status { get; private set; }

    public bool DueReminderSent { get; private set; }

    public DateTime? PaidAtUtc { get; private set; }

    public DateTime? MissedRecordedAtUtc { get; private set; }

    public static InstalmentPayment Schedule(
        Guid instalmentPlanId,
        Guid patientId,
        InstalmentScheduleEntry entry,
        string currency)
    {
        return new InstalmentPayment
        {
            InstalmentPlanId = instalmentPlanId,
            PatientId = patientId,
            SequenceNumber = entry.SequenceNumber,
            AmountMinorUnits = entry.AmountMinorUnits,
            Currency = currency.Trim().ToUpperInvariant(),
            DueDate = entry.DueDate,
            Status = InstalmentPaymentStatus.Scheduled,
            DueReminderSent = false
        };
    }

    public void MarkDueReminderSent()
    {
        DueReminderSent = true;
        Touch();
    }

    public void MarkPaid(DateTime paidAtUtc)
    {
        if (Status != InstalmentPaymentStatus.Scheduled)
        {
            throw new InvalidInstalmentPlanException("Only scheduled instalment payments can be marked paid.");
        }

        Status = InstalmentPaymentStatus.Paid;
        PaidAtUtc = paidAtUtc;
        Touch();
    }

    public long MarkMissed(long lateFeeMinorUnits, DateTime missedAtUtc)
    {
        if (Status != InstalmentPaymentStatus.Scheduled)
        {
            throw new InvalidInstalmentPlanException("Only scheduled instalment payments can be marked missed.");
        }

        Status = InstalmentPaymentStatus.Missed;
        LateFeeMinorUnits = lateFeeMinorUnits;
        MissedRecordedAtUtc = missedAtUtc;
        Touch();

        RaiseDomainEvent(new InstalmentPaymentMissedDomainEvent(
            Id,
            InstalmentPlanId,
            PatientId,
            SequenceNumber,
            AmountMinorUnits,
            lateFeeMinorUnits,
            Currency,
            DueDate));

        return lateFeeMinorUnits;
    }
}
