using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.NextOfKin.GetNextOfKinContact;

public sealed record GetNextOfKinContactQuery(Guid ContactId) : IQuery<NextOfKinContactDto>;
