using HealthPlatform.Application.Identity.Notifications;
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
            _ => Task.CompletedTask
        };
}
