using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Payments.Instalments.Events;

namespace HealthPlatform.Domain.Payments.Instalments;

public sealed class InstalmentPlan : Entity
{
    private InstalmentPlan()
    {
        Currency = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public long TotalAmountMinorUnits { get; private set; }

    public long InstalmentAmountMinorUnits { get; private set; }

    public long TotalRepayableMinorUnits { get; private set; }

    public InstalmentFrequency Frequency { get; private set; }

    public int TotalInstalments { get; private set; }

    public int PaidInstalments { get; private set; }

    public string Currency { get; private set; }

    public InstalmentPlanStatus Status { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public Guid? MedicationOrderId { get; private set; }

    public Guid? LabOrderId { get; private set; }

    public DateOnly FirstDueDate { get; private set; }

    public static InstalmentPlan Create(
        Guid patientId,
        long totalAmountMinorUnits,
        InstalmentFrequency frequency,
        int instalmentCount,
        string currency,
        DateOnly firstDueDate,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId,
        long minimumExpenseMinorUnits)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        if (!InstalmentPolicies.MeetsMinimumExpense(totalAmountMinorUnits, minimumExpenseMinorUnits))
        {
            throw new InstalmentExpenseBelowThresholdException(minimumExpenseMinorUnits);
        }

        ValidateTarget(appointmentId, medicationOrderId, labOrderId);

        var schedule = InstalmentPolicies.BuildSchedule(
            totalAmountMinorUnits,
            instalmentCount,
            frequency,
            firstDueDate);

        var plan = new InstalmentPlan
        {
            PatientId = patientId,
            TotalAmountMinorUnits = totalAmountMinorUnits,
            InstalmentAmountMinorUnits = schedule[0].AmountMinorUnits,
            TotalRepayableMinorUnits = totalAmountMinorUnits,
            Frequency = frequency,
            TotalInstalments = instalmentCount,
            PaidInstalments = 0,
            Currency = currency.Trim().ToUpperInvariant(),
            Status = InstalmentPlanStatus.Active,
            AppointmentId = appointmentId,
            MedicationOrderId = medicationOrderId,
            LabOrderId = labOrderId,
            FirstDueDate = firstDueDate
        };

        plan.RaiseDomainEvent(new InstalmentPlanCreatedDomainEvent(
            plan.Id,
            plan.PatientId,
            plan.TotalAmountMinorUnits,
            plan.Currency,
            plan.TotalInstalments));

        return plan;
    }

    public void ApplyLateFee(long lateFeeMinorUnits)
    {
        if (lateFeeMinorUnits < 0)
        {
            throw new ArgumentException("Late fee cannot be negative.", nameof(lateFeeMinorUnits));
        }

        TotalRepayableMinorUnits += lateFeeMinorUnits;
        Touch();
    }

    public void RecordInstalmentPaid()
    {
        if (Status != InstalmentPlanStatus.Active)
        {
            throw new InvalidInstalmentPlanException("Only active instalment plans can accept payments.");
        }

        PaidInstalments++;
        if (PaidInstalments >= TotalInstalments)
        {
            Status = InstalmentPlanStatus.Completed;
        }

        Touch();
    }

    public void MarkDefaulted()
    {
        Status = InstalmentPlanStatus.Defaulted;
        Touch();
    }

    private static void ValidateTarget(Guid? appointmentId, Guid? medicationOrderId, Guid? labOrderId)
    {
        var targetCount = new[] { appointmentId, medicationOrderId, labOrderId }.Count(id => id is not null);
        if (targetCount != 1)
        {
            throw new ArgumentException("Exactly one expense target id is required.");
        }
    }
}
