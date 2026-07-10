namespace HealthPlatform.API.Requests.Maternal;

public sealed record RecordGrowthEntryRequest(
    decimal? HeightCm,
    decimal? WeightKg,
    string? MilestoneNote,
    DateTime? RecordedAtUtc);
