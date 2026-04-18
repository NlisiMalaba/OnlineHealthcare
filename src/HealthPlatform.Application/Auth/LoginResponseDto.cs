namespace HealthPlatform.Application.Auth;

public enum LoginApiResult
{
    Authenticated,
    TwoFactorRequired,
    DeviceVerificationRequired
}

public sealed record LoginResponseDto(
    LoginApiResult Result,
    string? AccessToken,
    long? ExpiresInSeconds,
    string? MfaChallengeToken,
    long? MfaChallengeExpiresInSeconds,
    string? DeviceChallengeToken,
    long? DeviceChallengeExpiresInSeconds);
