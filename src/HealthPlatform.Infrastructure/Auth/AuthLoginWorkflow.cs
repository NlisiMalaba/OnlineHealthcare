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
    ILogger<AuthLoginWorkflow> logger) : IAuthLoginWorkflow
{
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
                jwtOptions.Value.MfaChallengeMinutes * 60L);
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
            null);
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
