namespace HealthPlatform.Infrastructure.Search.Documents;

/// <summary>
/// Elasticsearch read model for doctor discovery (Requirement 3).
/// </summary>
public sealed class DoctorSearchDocument
{
    public string DoctorId { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string Specialty { get; init; } = string.Empty;

    public double AverageRating { get; init; }

    public int TotalReviews { get; init; }

    public GeoLocationDocument? ClinicLocation { get; init; }

    public decimal VirtualFee { get; init; }

    public decimal PhysicalFee { get; init; }

    public decimal MinFee { get; init; }

    public decimal MaxFee { get; init; }

    public bool HasAvailability { get; init; }

    public IReadOnlyList<DoctorAvailabilitySlotDocument> Availability { get; init; } = [];

    public bool IsSearchable { get; init; }
}
