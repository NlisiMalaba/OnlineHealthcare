using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.NextOfKin.UpdateNextOfKinContact;

public sealed record UpdateNextOfKinContactCommand(
    Guid ContactId,
    string FullName,
    string Relationship,
    string PhoneNumber,
    string? Email,
    bool IsMentalHealthContact) : ICommand<NextOfKinContactDto>;
