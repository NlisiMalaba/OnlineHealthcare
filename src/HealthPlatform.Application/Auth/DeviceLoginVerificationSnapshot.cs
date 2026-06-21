namespace HealthPlatform.Application.Auth;

public sealed record DeviceLoginVerificationSnapshot(Guid Id, string OtpPasswordHash);
