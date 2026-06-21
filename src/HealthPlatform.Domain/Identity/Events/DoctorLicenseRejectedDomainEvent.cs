using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record DoctorLicenseRejectedDomainEvent(
    Guid DoctorId,
    Guid UserId,
    string FullName,
    string Reason) : DomainEvent;
