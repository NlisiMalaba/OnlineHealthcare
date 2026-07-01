using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.NextOfKin.ListNextOfKinContacts;

public sealed record ListNextOfKinContactsQuery : IQuery<IReadOnlyList<NextOfKinContactDto>>;
