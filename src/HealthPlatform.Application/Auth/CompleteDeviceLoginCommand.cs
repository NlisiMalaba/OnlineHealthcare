using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Auth;

public sealed record CompleteDeviceLoginCommand(
    string DeviceChallengeToken,
    string VerificationCode,
    string DeviceFingerprint) : ICommand<LoginResponseDto>;
