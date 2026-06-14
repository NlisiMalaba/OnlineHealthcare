using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public sealed record SocialIdentityVerificationResult(
    string ProviderKey,
    string? Email,
    string? FullName);

public interface ISocialIdentityVerifier
{
    Task<SocialIdentityVerificationResult> VerifyAsync(
        PatientAuthProvider provider,
        string idToken,
        CancellationToken ct);
}
