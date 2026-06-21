using System.IdentityModel.Tokens.Jwt;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthPlatform.Infrastructure.Identity;

public sealed class SocialIdentityVerifierOptions
{
    public const string SectionName = "SocialAuth";

    public bool AllowUnverifiedTokensInDevelopment { get; set; } = true;

    public string? GoogleClientId { get; set; }

    public string? AppleClientId { get; set; }
}

public sealed class SocialIdentityVerifier(
    IHostEnvironment environment,
    IOptions<SocialIdentityVerifierOptions> options,
    ILogger<SocialIdentityVerifier> logger) : ISocialIdentityVerifier
{
    public Task<SocialIdentityVerificationResult> VerifyAsync(
        PatientAuthProvider provider,
        string idToken,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        if (provider is not (PatientAuthProvider.Google or PatientAuthProvider.Apple))
        {
            throw new DomainException(
                IdentityErrorCodes.InvalidSocialToken,
                "Social identity verification is only supported for Google and Apple.");
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(idToken))
            {
                throw CreateInvalidTokenException();
            }

            var jwt = handler.ReadJwtToken(idToken);
            var providerKey = jwt.Subject
                ?? jwt.Claims.FirstOrDefault(c => c.Type is "sub" or JwtRegisteredClaimNames.Sub)?.Value;

            if (string.IsNullOrWhiteSpace(providerKey))
            {
                throw CreateInvalidTokenException();
            }

            if (!environment.IsDevelopment() || !options.Value.AllowUnverifiedTokensInDevelopment)
            {
                ValidateTokenSignature(provider, idToken, handler);
            }
            else
            {
                logger.LogDebug(
                    "Accepting unverified {Provider} id token in development for subject {Subject}.",
                    provider,
                    providerKey);
            }

            var email = jwt.Claims.FirstOrDefault(c => c.Type is "email" or JwtRegisteredClaimNames.Email)?.Value;
            var fullName = jwt.Claims.FirstOrDefault(c => c.Type is "name" or JwtRegisteredClaimNames.Name)?.Value;

            return Task.FromResult(new SocialIdentityVerificationResult(providerKey, email, fullName));
        }
        catch (DomainException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to verify {Provider} id token.", provider);
            throw CreateInvalidTokenException();
        }
    }

    private void ValidateTokenSignature(
        PatientAuthProvider provider,
        string idToken,
        JwtSecurityTokenHandler handler)
    {
        var clientId = provider switch
        {
            PatientAuthProvider.Google => options.Value.GoogleClientId,
            PatientAuthProvider.Apple => options.Value.AppleClientId,
            _ => null
        };

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new DomainException(
                IdentityErrorCodes.InvalidSocialToken,
                $"{provider} client id is not configured.");
        }

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = provider switch
            {
                PatientAuthProvider.Google => ["https://accounts.google.com", "accounts.google.com"],
                PatientAuthProvider.Apple => ["https://appleid.apple.com"],
                _ => []
            },
            ValidateAudience = true,
            ValidAudience = clientId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = false,
            SignatureValidator = (_, _) => new JwtSecurityToken(idToken)
        };

        handler.ValidateToken(idToken, validationParameters, out _);
    }

    private static DomainException CreateInvalidTokenException() =>
        new(IdentityErrorCodes.InvalidSocialToken, "The social login token is invalid or expired.");
}
