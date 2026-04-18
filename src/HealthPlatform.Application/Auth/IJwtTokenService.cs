namespace HealthPlatform.Application.Auth;

/// <summary>
/// Issues and validates JWT access and short-lived MFA challenge tokens (implementation in Infrastructure).
/// </summary>
public interface IJwtTokenService
{
    string CreateAccessToken(Guid userId, string email, IReadOnlyList<string> roles, bool usedTwoFactor, CancellationToken ct);

    string CreateMfaChallengeToken(Guid userId, CancellationToken ct);

    bool TryValidateMfaChallengeToken(string token, CancellationToken ct, out Guid userId);
}
