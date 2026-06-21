using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Auth;

public sealed record CompleteMfaLoginCommand(
    string MfaChallengeToken,
    string TwoFactorCode,
    string TwoFactorProvider,
    string DeviceFingerprint) : ICommand<LoginResponseDto>;
