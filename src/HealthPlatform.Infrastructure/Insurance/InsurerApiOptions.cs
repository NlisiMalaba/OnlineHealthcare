namespace HealthPlatform.Infrastructure.Insurance;

public sealed class InsurerApiOptions
{
    public const string SectionName = "Insurance:Insurers";

    public IList<InsurerEndpointOptions> Endpoints { get; set; } = [];
}

public sealed class InsurerEndpointOptions
{
    public string Code { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public string? BaseUrl { get; set; }

    public string? ApiKey { get; set; }

    public string? WebhookSecret { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
