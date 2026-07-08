namespace HealthPlatform.Application.Audit;

public interface IAuditContextAccessor
{
    string? IpAddress { get; }

    string? UserAgent { get; }
}
