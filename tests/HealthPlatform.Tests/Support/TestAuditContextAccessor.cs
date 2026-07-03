using HealthPlatform.Application.Audit;

namespace HealthPlatform.Tests.Support;

public sealed class TestAuditContextAccessor : IAuditContextAccessor
{
    public string? IpAddress { get; set; } = "127.0.0.1";

    public string? UserAgent { get; set; } = "HealthPlatform.Tests";
}
