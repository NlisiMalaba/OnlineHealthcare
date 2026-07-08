using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.HealthRecords.ListHealthRecordAccessGrants;

public sealed record ListHealthRecordAccessGrantsQuery() : IQuery<IReadOnlyList<HealthRecordAccessDto>>;
