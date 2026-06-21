using HealthPlatform.Application.Identity;

namespace HealthPlatform.Tests.Support;

public sealed class TestCurrentUserAccessor : ICurrentUserAccessor
{
    public Guid? UserId { get; set; }
}
