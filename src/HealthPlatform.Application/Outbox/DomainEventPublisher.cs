using HealthPlatform.Application.Appointments.Notifications;
using HealthPlatform.Application.Identity.Notifications;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Search.Notifications;
using HealthPlatform.Domain.Appointments.Events;
using HealthPlatform.Domain.Events;
using HealthPlatform.Domain.Identity.Events;
using MediatR;

namespace HealthPlatform.Application.Outbox;

public sealed class DomainEventPublisher(IMediator mediator) : IDomainEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken ct) =>
        domainEvent switch
        {
            AccountLockedDomainEvent e => mediator.Publish(
                new AccountLockedNotification(e.UserId, e.LockoutEndUtc, e.FailedAttemptCount),
                ct),
            PatientRegisteredDomainEvent e => mediator.Publish(
                new PatientRegisteredNotification(e.PatientId, e.OccurredAtUtc),
                ct),
            DoctorRegisteredDomainEvent e => mediator.Publish(
                new DoctorRegisteredNotification(
                    e.DoctorId,
                    e.LicenseNumber,
                    e.FullName,
                    e.OccurredAtUtc),
                ct),
            DoctorLicenseVerifiedDomainEvent e => mediator.Publish(
                new DoctorLicenseVerifiedNotification(
                    e.DoctorId,
                    e.UserId,
                    e.FullName,
                    e.OccurredAtUtc),
                ct),
            DoctorLicenseRejectedDomainEvent e => mediator.Publish(
                new DoctorLicenseRejectedNotification(
                    e.DoctorId,
                    e.UserId,
                    e.FullName,
                    e.Reason,
                    e.OccurredAtUtc),
                ct),
            DoctorAvailabilityChangedDomainEvent e => mediator.Publish(
                new DoctorAvailabilityChangedNotification(e.DoctorId, e.OccurredAtUtc),
                ct),
            PaymentCompletedDomainEvent e => mediator.Publish(
                new PaymentCompletedNotification(e.AppointmentId, e.PaymentId, e.OccurredAtUtc),
                ct),
            AppointmentConfirmedDomainEvent e => mediator.Publish(
                new AppointmentConfirmedNotification(
                    e.AppointmentId,
                    e.PatientId,
                    e.DoctorId,
                    e.ScheduledAtUtc,
                    e.ConfirmedAtUtc,
                    e.OccurredAtUtc),
                ct),
            DoctorProfileUpdatedDomainEvent e => mediator.Publish(
                new DoctorProfileUpdatedNotification(e.DoctorId, e.OccurredAtUtc),
                ct),
            PharmacyRegisteredDomainEvent e => mediator.Publish(
                new PharmacyRegisteredSearchNotification(e.PharmacyId, e.OccurredAtUtc),
                ct),
            PharmacyProfileUpdatedDomainEvent e => mediator.Publish(
                new PharmacyProfileUpdatedNotification(e.PharmacyId, e.OccurredAtUtc),
                ct),
            PharmacyStockChangedDomainEvent e => mediator.Publish(
                new PharmacyStockChangedNotification(e.PharmacyId, e.StockSummary, e.OccurredAtUtc),
                ct),
            _ => Task.CompletedTask
        };
}
