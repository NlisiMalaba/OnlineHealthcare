using HealthPlatform.Application.Audit;
using Microsoft.AspNetCore.Http;

namespace HealthPlatform.Infrastructure.Audit;

public sealed class HttpAuditContextAccessor(IHttpContextAccessor httpContextAccessor) : IAuditContextAccessor
{
    public string? IpAddress =>
        httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        httpContextAccessor.HttpContext?.Request.Headers.UserAgent.ToString();
}
