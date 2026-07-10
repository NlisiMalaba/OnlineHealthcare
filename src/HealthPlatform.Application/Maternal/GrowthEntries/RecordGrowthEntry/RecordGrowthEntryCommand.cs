using HealthPlatform.Application.Maternal.GrowthEntries;
using MediatR;

namespace HealthPlatform.Application.Maternal.GrowthEntries.RecordGrowthEntry;

public sealed record RecordGrowthEntryCommand(
    Guid ChildProfileId,
    decimal? HeightCm,
    decimal? WeightKg,
    string? MilestoneNote,
    DateTime? RecordedAtUtc) : IRequest<GrowthEntryDto>;
