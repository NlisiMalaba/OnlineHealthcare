namespace HealthPlatform.Application.Identity;

public interface ICurrentUserAccessor
{
    Guid? UserId { get; }
}
