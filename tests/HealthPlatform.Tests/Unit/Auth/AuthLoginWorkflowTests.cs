using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Security;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.Auth;

public sealed class AuthLoginWorkflowTests
{
    [Fact]
    public async Task LoginAsync_unrecognized_device_triggers_device_verification_step_up()
    {
        await using var host = new AuthTestHost();
        var email = $"patient-device-{Guid.NewGuid():N}@test.local";
        await host.CreateUserAsync(email, AuthTestHost.ValidPassword, [ApplicationRoles.Patient]);

        var response = await host.LoginAsync(
            new LoginCommand(email, AuthTestHost.ValidPassword, AuthTestHost.DeviceFingerprint));

        Assert.Equal(LoginApiResult.DeviceVerificationRequired, response.Result);
        Assert.Null(response.AccessToken);
        Assert.NotNull(response.DeviceChallengeToken);
        Assert.True(response.DeviceChallengeExpiresInSeconds > 0);
        Assert.NotNull(host.DeviceOtpNotifier.LastCode);
        Assert.Equal(email, host.DeviceOtpNotifier.LastEmail);
    }

    [Fact]
    public async Task LoginAsync_trusted_device_returns_access_token_without_step_up()
    {
        await using var host = new AuthTestHost();
        var email = $"patient-trusted-{Guid.NewGuid():N}@test.local";
        var user = await host.CreateUserAsync(email, AuthTestHost.ValidPassword, [ApplicationRoles.Patient]);
        await host.TrustDeviceAsync(user.Id, AuthTestHost.DeviceFingerprint);

        var response = await host.LoginAsync(
            new LoginCommand(email, AuthTestHost.ValidPassword, AuthTestHost.DeviceFingerprint));

        Assert.Equal(LoginApiResult.Authenticated, response.Result);
        Assert.NotNull(response.AccessToken);
        Assert.Null(response.DeviceChallengeToken);
    }

    [Fact]
    public async Task LoginAsync_mandatory_mfa_role_without_enrollment_rejects_bypass()
    {
        await using var host = new AuthTestHost();
        var email = $"doctor-no-mfa-{Guid.NewGuid():N}@test.local";
        var user = await host.CreateUserAsync(
            email,
            AuthTestHost.ValidPassword,
            [ApplicationRoles.Doctor],
            twoFactorEnabled: false);
        await host.TrustDeviceAsync(user.Id, AuthTestHost.DeviceFingerprint);

        var ex = await Assert.ThrowsAsync<AppHttpException>(() =>
            host.LoginAsync(new LoginCommand(email, AuthTestHost.ValidPassword, AuthTestHost.DeviceFingerprint)));

        Assert.Equal(403, ex.StatusCode);
        Assert.Equal("MFA_ENROLLMENT_REQUIRED", ex.ErrorCode);
    }

    [Fact]
    public async Task LoginAsync_two_factor_enabled_user_does_not_receive_access_token_from_password_alone()
    {
        await using var host = new AuthTestHost();
        var email = $"doctor-mfa-{Guid.NewGuid():N}@test.local";
        await host.CreateUserAsync(
            email,
            AuthTestHost.ValidPassword,
            [ApplicationRoles.Doctor],
            twoFactorEnabled: true);

        Assert.True(await host.IsTwoFactorEnabledAsync(email));

        var response = await host.LoginAsync(
            new LoginCommand(email, AuthTestHost.ValidPassword, AuthTestHost.DeviceFingerprint));

        Assert.Null(response.AccessToken);
        Assert.NotEqual(LoginApiResult.Authenticated, response.Result);
    }

    [Fact]
    public async Task CompleteMfaAsync_invalid_challenge_token_rejects_bypass()
    {
        await using var host = new AuthTestHost();
        var email = $"doctor-mfa-bypass-{Guid.NewGuid():N}@test.local";
        await host.CreateUserAsync(
            email,
            AuthTestHost.ValidPassword,
            [ApplicationRoles.Doctor],
            twoFactorEnabled: true);

        var ex = await Assert.ThrowsAsync<AppHttpException>(() =>
            host.CompleteMfaAsync(
                new CompleteMfaLoginCommand(
                    "not-a-valid-mfa-challenge-token",
                    "123456",
                    TwoFactorProviders.Authenticator,
                    AuthTestHost.DeviceFingerprint)));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("INVALID_MFA_CHALLENGE", ex.ErrorCode);
    }

    [Fact]
    public async Task CompleteMfaAsync_invalid_two_factor_code_rejects_bypass()
    {
        await using var host = new AuthTestHost();
        var email = $"doctor-mfa-code-{Guid.NewGuid():N}@test.local";
        var user = await host.CreateUserAsync(
            email,
            AuthTestHost.ValidPassword,
            [ApplicationRoles.Doctor],
            twoFactorEnabled: true);

        var mfaToken = await host.CreateMfaChallengeTokenAsync(user.Id);

        var ex = await Assert.ThrowsAsync<AppHttpException>(() =>
            host.CompleteMfaAsync(
                new CompleteMfaLoginCommand(
                    mfaToken,
                    "000000",
                    TwoFactorProviders.Authenticator,
                    AuthTestHost.DeviceFingerprint)));

        Assert.Equal(401, ex.StatusCode);
        Assert.Equal("INVALID_TWO_FACTOR_CODE", ex.ErrorCode);
    }
}
