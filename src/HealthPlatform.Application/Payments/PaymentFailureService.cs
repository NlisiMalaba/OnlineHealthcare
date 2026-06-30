using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.PharmacyOrders;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Pharmacy;

namespace HealthPlatform.Application.Payments;

public sealed class PaymentFailureService(
    IPaymentRepository paymentRepository,
    IAppointmentRepository appointmentRepository,
    IMedicationOrderRepository medicationOrderRepository,
    ISlotHoldService slotHoldService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IPaymentFailureService
{
    public async Task<PaymentFailureResultDto> RecordFailureAsync(
        RecordPaymentFailureRequest request,
        CancellationToken ct)
    {
        var retentionExpiresAtUtc = request.FailedAtUtc.Add(PaymentPolicies.PendingRetentionWindow);
        var payment = Payment.RecordFailure(
            request.PatientId,
            request.AmountMinorUnits,
            request.Currency,
            request.PaymentMethod,
            request.Gateway,
            request.GatewayReference,
            request.AppointmentId,
            request.MedicationOrderId,
            request.LabOrderId,
            request.FailureCode,
            request.FailureMessage,
            request.FailedAtUtc,
            retentionExpiresAtUtc);

        if (request.AppointmentId is { } appointmentId)
        {
            await RetainAppointmentPendingAsync(appointmentId, request.FailedAtUtc, ct);
        }

        if (request.MedicationOrderId is { } medicationOrderId)
        {
            await RetainMedicationOrderPendingAsync(medicationOrderId, request.FailedAtUtc, ct);
        }

        var pendingEvents = payment.DomainEvents.ToList();
        await paymentRepository.AddAsync(payment, ct);

        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        payment.ClearDomainEvents();
        await paymentRepository.SaveChangesAsync(ct);

        return new PaymentFailureResultDto(payment.Id, retentionExpiresAtUtc);
    }

    private async Task RetainAppointmentPendingAsync(
        Guid appointmentId,
        DateTime failedAtUtc,
        CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, ct);
        if (appointment is null || appointment.Status != AppointmentStatus.PendingPayment)
        {
            return;
        }

        var retentionExpiresAtUtc = appointment.RetainPendingAfterPaymentFailure(
            failedAtUtc,
            PaymentPolicies.PendingRetentionWindow);

        var holdTtl = retentionExpiresAtUtc - timeProvider.GetUtcNow().UtcDateTime;
        if (holdTtl > TimeSpan.Zero)
        {
            await slotHoldService.ExtendHoldAsync(appointment.SlotId, holdTtl, ct);
        }
        await appointmentRepository.UpdateAsync(appointment, ct);
    }

    private async Task RetainMedicationOrderPendingAsync(
        Guid medicationOrderId,
        DateTime failedAtUtc,
        CancellationToken ct)
    {
        var order = await medicationOrderRepository.GetByIdAsync(medicationOrderId, ct);
        if (order is null || order.Status != MedicationOrderStatus.Pending)
        {
            return;
        }

        order.RetainPendingAfterPaymentFailure(failedAtUtc, PaymentPolicies.PendingRetentionWindow);
        await medicationOrderRepository.UpdateAsync(order, ct);
    }
}
