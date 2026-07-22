namespace HealthPlatform.API.Requests.Wellness;

public sealed class UpdateHealthGoalRequest
{
    public required decimal TargetValue { get; init; }

    public required string Unit { get; init; }

    public string? CustomLabel { get; init; }
}
