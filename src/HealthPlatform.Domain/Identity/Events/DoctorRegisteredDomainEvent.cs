using HealthPlatform.Domain.Events;

namespace HealthPlatform.Domain.Identity.Events;

public sealed record DoctorRegisteredDomainEvent(
    Guid DoctorId,
    string LicenseNumber,
    string FullName) : DomainEvent;
