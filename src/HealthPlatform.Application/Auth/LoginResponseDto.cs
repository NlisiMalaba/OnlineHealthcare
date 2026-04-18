namespace HealthPlatform.Application.Auth;

public enum LoginApiResult
{
    Authenticated,
    TwoFactorRequired
}

public sealed record LoginResponseDto(
    LoginApiResult Result,
    string? AccessToken,
    long? ExpiresInSeconds,
    string? MfaChallengeToken,
    long? MfaChallengeExpiresInSeconds);
