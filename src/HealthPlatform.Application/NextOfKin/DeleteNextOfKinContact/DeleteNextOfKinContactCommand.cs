using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.NextOfKin.DeleteNextOfKinContact;

public sealed record DeleteNextOfKinContactCommand(Guid ContactId) : ICommand;
