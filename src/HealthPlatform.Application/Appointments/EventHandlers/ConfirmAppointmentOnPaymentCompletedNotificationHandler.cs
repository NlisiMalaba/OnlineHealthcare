using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Outbox;
using MediatR;

namespace HealthPlatform.Application.Appointments.EventHandlers;

public sealed class ConfirmAppointmentOnPaymentCompletedNotificationHandler(
    IAppointmentRepository appointmentRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : INotificationHandler<PaymentCompletedNotification>
{
    public async Task Handle(PaymentCompletedNotification notification, CancellationToken ct)
    {
        var appointment = await appointmentRepository.GetByIdAsync(notification.AppointmentId, ct)
            ?? throw new NotFoundException(
                "APPOINTMENT_NOT_FOUND",
                "Appointment was not found.");

        appointment.ConfirmOnPayment(notification.OccurredAtUtc);
        await appointmentRepository.UpdateAsync(appointment, ct);

        var pendingEvents = appointment.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        appointment.ClearDomainEvents();
    }
}
