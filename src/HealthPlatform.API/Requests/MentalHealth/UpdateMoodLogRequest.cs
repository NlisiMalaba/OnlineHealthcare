namespace HealthPlatform.API.Requests.MentalHealth;

public sealed class UpdateMoodLogRequest
{
    public int Rating { get; init; }

    public string? Notes { get; init; }

    public DateTime? LoggedAtUtc { get; init; }
}
