namespace HealthPlatform.API.Requests.Maternal;

public sealed record CreateChildProfileRequest(
    string FullName,
    DateOnly DateOfBirth,
    string? BloodType,
    IReadOnlyList<string> KnownAllergies);
