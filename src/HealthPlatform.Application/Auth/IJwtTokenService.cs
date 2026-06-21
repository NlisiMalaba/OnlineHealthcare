namespace HealthPlatform.Application.Auth;

/// <summary>
/// Issues and validates JWT access and short-lived MFA challenge tokens (implementation in Infrastructure).
/// </summary>
public interface IJwtTokenService
{
    string CreateAccessToken(Guid userId, string email, IReadOnlyList<string> roles, bool usedTwoFactor, CancellationToken ct);

    string CreateMfaChallengeToken(Guid userId, CancellationToken ct);

    bool TryValidateMfaChallengeToken(string token, CancellationToken ct, out Guid userId);

    string CreateDeviceLoginChallengeToken(
        Guid userId,
        Guid verificationId,
        int ttlMinutes,
        bool twoFactorAlreadySatisfied,
        CancellationToken ct);

    bool TryValidateDeviceLoginChallengeToken(
        string token,
        CancellationToken ct,
        out Guid userId,
        out Guid verificationId,
        out bool twoFactorAlreadySatisfied);
}
