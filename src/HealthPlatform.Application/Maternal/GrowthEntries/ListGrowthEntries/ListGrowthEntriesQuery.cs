using HealthPlatform.Application.Maternal.GrowthEntries;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.ListGrowthEntries;

public sealed record ListGrowthEntriesQuery(Guid ChildProfileId) : IRequest<IReadOnlyList<GrowthEntryDto>>;
