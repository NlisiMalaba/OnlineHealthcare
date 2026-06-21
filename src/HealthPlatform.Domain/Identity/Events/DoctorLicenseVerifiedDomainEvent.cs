using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record DoctorLicenseVerifiedDomainEvent(
    Guid DoctorId,
    Guid UserId,
    string FullName) : DomainEvent;
