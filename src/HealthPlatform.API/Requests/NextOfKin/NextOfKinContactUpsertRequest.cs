namespace HealthPlatform.API.Requests.NextOfKin;

public sealed class NextOfKinContactUpsertRequest
{
    public required string FullName { get; init; }

    public required string Relationship { get; init; }

    public required string PhoneNumber { get; init; }

    public string? Email { get; init; }

    public bool IsMentalHealthContact { get; init; }
}
