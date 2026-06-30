using HealthPlatform.Domain.Appointments.Events;
using HealthPlatform.Domain.Common;
using HealthPlatform.Domain.Payments.Events;

namespace HealthPlatform.Domain.Payments;

public sealed class Payment : Entity
{
    private Payment()
    {
        Currency = string.Empty;
    }

    public Guid PatientId { get; private set; }

    public long AmountMinorUnits { get; private set; }

    public string Currency { get; private set; }

    public PaymentMethod PaymentMethod { get; private set; }

    public PaymentGatewayType Gateway { get; private set; }

    public string? GatewayReference { get; private set; }

    public PaymentStatus Status { get; private set; }

    public string? ReceiptStorageKey { get; private set; }

    public Guid? AppointmentId { get; private set; }

    public Guid? MedicationOrderId { get; private set; }

    public Guid? LabOrderId { get; private set; }

    public DateTime? RetentionExpiresAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public DateTime? FailedAtUtc { get; private set; }

    public string? FailureCode { get; private set; }

    public string? FailureMessage { get; private set; }

    public static Payment RecordCompletion(
        Guid patientId,
        long amountMinorUnits,
        string currency,
        PaymentMethod paymentMethod,
        PaymentGatewayType gateway,
        string? gatewayReference,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId,
        DateTime completedAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (amountMinorUnits <= 0)
        {
            throw new ArgumentException("Amount must be positive.", nameof(amountMinorUnits));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var payment = new Payment
        {
            PatientId = patientId,
            AmountMinorUnits = amountMinorUnits,
            Currency = currency.Trim().ToUpperInvariant(),
            PaymentMethod = paymentMethod,
            Gateway = gateway,
            GatewayReference = string.IsNullOrWhiteSpace(gatewayReference) ? null : gatewayReference.Trim(),
            Status = PaymentStatus.Completed,
            AppointmentId = appointmentId,
            MedicationOrderId = medicationOrderId,
            LabOrderId = labOrderId,
            CompletedAtUtc = completedAtUtc,
            CreatedAtUtc = completedAtUtc,
            UpdatedAtUtc = completedAtUtc
        };

        if (appointmentId is { } resolvedAppointmentId)
        {
            payment.RaiseDomainEvent(new PaymentCompletedDomainEvent(resolvedAppointmentId, payment.Id));
        }

        return payment;
    }

    public static Payment RecordFailure(
        Guid patientId,
        long amountMinorUnits,
        string currency,
        PaymentMethod paymentMethod,
        PaymentGatewayType gateway,
        string? gatewayReference,
        Guid? appointmentId,
        Guid? medicationOrderId,
        Guid? labOrderId,
        string failureCode,
        string failureMessage,
        DateTime failedAtUtc,
        DateTime retentionExpiresAtUtc)
    {
        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (amountMinorUnits <= 0)
        {
            throw new ArgumentException("Amount must be positive.", nameof(amountMinorUnits));
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency is required.", nameof(currency));
        }

        var payment = new Payment
        {
            PatientId = patientId,
            AmountMinorUnits = amountMinorUnits,
            Currency = currency.Trim().ToUpperInvariant(),
            PaymentMethod = paymentMethod,
            Gateway = gateway,
            GatewayReference = string.IsNullOrWhiteSpace(gatewayReference) ? null : gatewayReference.Trim(),
            Status = PaymentStatus.Failed,
            AppointmentId = appointmentId,
            MedicationOrderId = medicationOrderId,
            LabOrderId = labOrderId,
            FailedAtUtc = failedAtUtc,
            RetentionExpiresAtUtc = retentionExpiresAtUtc,
            FailureCode = string.IsNullOrWhiteSpace(failureCode) ? "PAYMENT_FAILED" : failureCode.Trim(),
            FailureMessage = string.IsNullOrWhiteSpace(failureMessage)
                ? "Payment could not be completed."
                : failureMessage.Trim(),
            CreatedAtUtc = failedAtUtc,
            UpdatedAtUtc = failedAtUtc
        };

        payment.RaiseDomainEvent(new PaymentFailedDomainEvent(
            payment.Id,
            patientId,
            appointmentId,
            medicationOrderId,
            labOrderId,
            payment.FailureCode,
            payment.FailureMessage,
            retentionExpiresAtUtc));

        return payment;
    }

    public void AttachReceipt(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
        {
            throw new ArgumentException("Receipt storage key is required.", nameof(storageKey));
        }

        ReceiptStorageKey = storageKey.Trim();
        Touch();
    }
}
