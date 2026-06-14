using System.Security.Claims;
using HealthPlatform.Application.Identity;
using Microsoft.AspNetCore.Http;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class HttpCurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid? UserId
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            var subject = principal?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal?.FindFirstValue(ClaimTypes.Name)
                ?? principal?.FindFirstValue("sub");

            return Guid.TryParse(subject, out var userId) ? userId : null;
        }
    }
}
