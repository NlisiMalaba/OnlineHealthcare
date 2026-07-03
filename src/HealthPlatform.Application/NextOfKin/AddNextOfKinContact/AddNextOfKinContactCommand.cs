using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.NextOfKin.AddNextOfKinContact;

public sealed record AddNextOfKinContactCommand(
    string FullName,
    string Relationship,
    string PhoneNumber,
    string? Email,
    bool IsMentalHealthContact) : ICommand<NextOfKinContactDto>;
