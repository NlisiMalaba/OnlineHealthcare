using System.Globalization;
using System.Security.Cryptography;
using HealthPlatform.Application.Auth;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Security;
using HealthPlatform.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Auth;

public sealed class AuthLoginWorkflow(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IJwtTokenService jwt,
    IOptions<JwtOptions> jwtOptions,
    IOptions<DeviceLoginOptions> deviceLoginOptions,
    IUserDeviceFingerprintRepository fingerprintRepository,
    IDeviceLoginVerificationRepository verificationRepository,
    IDeviceLoginOtpNotifier deviceLoginOtpNotifier,
    ILogger<AuthLoginWorkflow> logger) : IAuthLoginWorkflow
{
    private static readonly object OtpMarker = new();
    private readonly PasswordHasher<object> _otpHasher = new();

    public async Task<LoginResponseDto> LoginAsync(LoginCommand command, CancellationToken ct)
    {
        var user = await userManager.FindByEmailAsync(command.Email.Trim());
        if (user is null)
        {
            throw new AppHttpException(401, "INVALID_CREDENTIALS", "Invalid email or password.");
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, command.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            throw new AppHttpException(423, "ACCOUNT_LOCKED", "Account is locked. Try again later.");
        }

        if (result.IsNotAllowed)
        {
            throw new AppHttpException(403, "ACCOUNT_NOT_ALLOWED", "This account is not permitted to sign in.");
        }

        if (result.RequiresTwoFactor)
        {
            var mfaToken = jwt.CreateMfaChallengeToken(user.Id, ct);
            return new LoginResponseDto(
                LoginApiResult.TwoFactorRequired,
                null,
                null,
                mfaToken,
                jwtOptions.Value.MfaChallengeMinutes * 60L,
                null,
                null);
        }

        if (!result.Succeeded)
        {
            throw new AppHttpException(401, "INVALID_CREDENTIALS", "Invalid email or password.");
        }

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        if (RequiresMfaEnrollment(roles, user))
        {
            throw new AppHttpException(
                403,
                "MFA_ENROLLMENT_REQUIRED",
                "MFA (authenticator app or SMS) must be enabled before signing in.");
        }

        var fingerprintHash = DeviceFingerprintHasher.Hash(command.DeviceFingerprint);
        if (!await fingerprintRepository.ExistsAsync(user.Id, fingerprintHash, ct))
        {
            return await StartDeviceStepUpAsync(
                user,
                command.Email.Trim(),
                fingerprintHash,
                twoFactorAlreadySatisfied: false,
                ct);
        }

        await fingerprintRepository.UpsertTouchAsync(user.Id, fingerprintHash, ct);

        var access = jwt.CreateAccessToken(
            user.Id,
            user.Email ?? command.Email.Trim(),
            roles,
            usedTwoFactor: false,
            ct);

        return new LoginResponseDto(
            LoginApiResult.Authenticated,
            access,
            jwtOptions.Value.AccessTokenMinutes * 60L,
            null,
            null,
            null,
            null);
    }

    public async Task<LoginResponseDto> CompleteMfaAsync(CompleteMfaLoginCommand command, CancellationToken ct)
    {
        if (!jwt.TryValidateMfaChallengeToken(command.MfaChallengeToken, ct, out var userId))
        {
            throw new AppHttpException(401, "INVALID_MFA_CHALLENGE", "The MFA challenge is invalid or expired.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new AppHttpException(401, "INVALID_MFA_CHALLENGE", "The MFA challenge is invalid or expired.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new AppHttpException(423, "ACCOUNT_LOCKED", "Account is locked. Try again later.");
        }

        var provider = MapProvider(command.TwoFactorProvider);
        var valid = await userManager.VerifyTwoFactorTokenAsync(user, provider, command.TwoFactorCode);
        if (!valid)
        {
            await userManager.AccessFailedAsync(user);
            throw new AppHttpException(401, "INVALID_TWO_FACTOR_CODE", "The verification code was not valid.");
        }

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        if (RequiresMfaEnrollment(roles, user))
        {
            logger.LogWarning("User {UserId} completed 2FA but still lacks mandatory enrollment state.", user.Id);
            throw new AppHttpException(
                403,
                "MFA_ENROLLMENT_REQUIRED",
                "MFA (authenticator app or SMS) must be enabled before signing in.");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var fingerprintHash = DeviceFingerprintHasher.Hash(command.DeviceFingerprint);
        if (!await fingerprintRepository.ExistsAsync(user.Id, fingerprintHash, ct))
        {
            return await StartDeviceStepUpAsync(
                user,
                user.Email ?? string.Empty,
                fingerprintHash,
                twoFactorAlreadySatisfied: true,
                ct);
        }

        await fingerprintRepository.UpsertTouchAsync(user.Id, fingerprintHash, ct);

        var access = jwt.CreateAccessToken(
            user.Id,
            user.Email ?? string.Empty,
            roles,
            usedTwoFactor: true,
            ct);

        return new LoginResponseDto(
            LoginApiResult.Authenticated,
            access,
            jwtOptions.Value.AccessTokenMinutes * 60L,
            null,
            null,
            null,
            null);
    }

    public async Task<LoginResponseDto> CompleteDeviceLoginAsync(CompleteDeviceLoginCommand command, CancellationToken ct)
    {
        if (!jwt.TryValidateDeviceLoginChallengeToken(
                command.DeviceChallengeToken,
                ct,
                out var userId,
                out var verificationId,
                out var twoFactorAlreadySatisfied))
        {
            throw new AppHttpException(
                401,
                "INVALID_DEVICE_CHALLENGE",
                "The device verification challenge is invalid or expired.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new AppHttpException(
                401,
                "INVALID_DEVICE_CHALLENGE",
                "The device verification challenge is invalid or expired.");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            throw new AppHttpException(423, "ACCOUNT_LOCKED", "Account is locked. Try again later.");
        }

        var fingerprintHash = DeviceFingerprintHasher.Hash(command.DeviceFingerprint);
        var snapshot = await verificationRepository.FindConsumableAsync(verificationId, userId, fingerprintHash, ct);
        if (snapshot is null)
        {
            throw new AppHttpException(
                401,
                "INVALID_DEVICE_VERIFICATION",
                "Device verification is no longer valid.");
        }

        var otpCheck = _otpHasher.VerifyHashedPassword(OtpMarker, snapshot.OtpPasswordHash, command.VerificationCode.Trim());
        if (otpCheck == PasswordVerificationResult.Failed)
        {
            throw new AppHttpException(
                401,
                "INVALID_DEVICE_VERIFICATION_CODE",
                "The verification code was not valid.");
        }

        await verificationRepository.MarkConsumedAsync(snapshot.Id, ct);
        await fingerprintRepository.UpsertTouchAsync(userId, fingerprintHash, ct);

        var roles = (await userManager.GetRolesAsync(user)).ToList();
        if (RequiresMfaEnrollment(roles, user))
        {
            throw new AppHttpException(
                403,
                "MFA_ENROLLMENT_REQUIRED",
                "MFA (authenticator app or SMS) must be enabled before signing in.");
        }

        var access = jwt.CreateAccessToken(
            user.Id,
            user.Email ?? string.Empty,
            roles,
            twoFactorAlreadySatisfied,
            ct);

        return new LoginResponseDto(
            LoginApiResult.Authenticated,
            access,
            jwtOptions.Value.AccessTokenMinutes * 60L,
            null,
            null,
            null,
            null);
    }

    private async Task<LoginResponseDto> StartDeviceStepUpAsync(
        ApplicationUser user,
        string emailForDelivery,
        string fingerprintHash,
        bool twoFactorAlreadySatisfied,
        CancellationToken ct)
    {
        var ttl = deviceLoginOptions.Value.ChallengeTtlMinutes;
        var code = RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6", CultureInfo.InvariantCulture);
        var otpHash = _otpHasher.HashPassword(OtpMarker, code);
        var expires = DateTimeOffset.UtcNow.AddMinutes(ttl);
        var verificationId = await verificationRepository.CreateAsync(user.Id, fingerprintHash, otpHash, expires, ct);

        await deviceLoginOtpNotifier.NotifyDeviceLoginCodeAsync(user.Email ?? emailForDelivery, code, ct);

        var deviceToken = jwt.CreateDeviceLoginChallengeToken(
            user.Id,
            verificationId,
            ttl,
            twoFactorAlreadySatisfied,
            ct);

        return new LoginResponseDto(
            LoginApiResult.DeviceVerificationRequired,
            null,
            null,
            null,
            null,
            deviceToken,
            ttl * 60L);
    }

    private static bool RequiresMfaEnrollment(IReadOnlyList<string> roles, ApplicationUser user)
    {
        if (user.TwoFactorEnabled)
        {
            return false;
        }

        return roles.Any(static r => ApplicationRoles.IsMandatoryTwoFactorRole(r));
    }

    private static string MapProvider(string provider) =>
        string.Equals(provider, TwoFactorProviders.Phone, StringComparison.Ordinal)
            ? TokenOptions.DefaultPhoneProvider
            : TokenOptions.DefaultAuthenticatorProvider;
}
