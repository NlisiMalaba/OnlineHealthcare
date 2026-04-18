using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthPlatform.Application.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthPlatform.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private const string TokenUsageClaim = "token_usage";
    private const string MfaChallengeValue = "mfa_challenge";
    private const string DeviceLoginChallengeValue = "device_login";
    private const string DeviceVerificationIdClaim = "dev_ver";

    private const string TwoFactorSatisfiedClaim = "tf_sat";

    public string CreateAccessToken(
        Guid userId,
        string email,
        IReadOnlyList<string> roles,
        bool usedTwoFactor,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var jwt = options.Value;
        var signingKey = CreateSigningKey(jwt.SigningKey);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (usedTwoFactor)
        {
            claims.Add(new Claim(
                "http://schemas.microsoft.com/claims/authnmethodsreferences",
                "mfa"));
        }

        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            claims,
            now,
            now.AddMinutes(jwt.AccessTokenMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateMfaChallengeToken(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        var jwt = options.Value;
        var signingKey = CreateSigningKey(jwt.SigningKey);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(TokenUsageClaim, MfaChallengeValue),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString())
        };

        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            claims,
            now,
            now.AddMinutes(jwt.MfaChallengeMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryValidateMfaChallengeToken(string token, CancellationToken ct, out Guid userId)
    {
        userId = default;
        ct.ThrowIfCancellationRequested();
        var jwt = options.Value;
        var signingKey = CreateSigningKey(jwt.SigningKey);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                return false;
            }

            var usage = principal.FindFirst(TokenUsageClaim)?.Value;
            if (!string.Equals(usage, MfaChallengeValue, StringComparison.Ordinal))
            {
                return false;
            }

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return sub is not null && Guid.TryParse(sub, out userId);
        }
        catch
        {
            return false;
        }
    }

    private static SymmetricSecurityKey CreateSigningKey(string signingKeyUtf8)
    {
        if (string.IsNullOrWhiteSpace(signingKeyUtf8))
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured.");
        }

        var bytes = Encoding.UTF8.GetBytes(signingKeyUtf8);
        if (bytes.Length < 32)
        {
            throw new InvalidOperationException("Jwt:SigningKey must be at least 32 UTF-8 bytes for HS256.");
        }

        return new SymmetricSecurityKey(bytes);
    }

    public string CreateDeviceLoginChallengeToken(
        Guid userId,
        Guid verificationId,
        int ttlMinutes,
        bool twoFactorAlreadySatisfied,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (ttlMinutes < 1 || ttlMinutes > 120)
        {
            throw new ArgumentOutOfRangeException(nameof(ttlMinutes));
        }

        var jwt = options.Value;
        var signingKey = CreateSigningKey(jwt.SigningKey);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(TokenUsageClaim, DeviceLoginChallengeValue),
            new(DeviceVerificationIdClaim, verificationId.ToString("D")),
            new(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new(TwoFactorSatisfiedClaim, twoFactorAlreadySatisfied ? "1" : "0")
        };

        var token = new JwtSecurityToken(
            jwt.Issuer,
            jwt.Audience,
            claims,
            now,
            now.AddMinutes(ttlMinutes),
            credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public bool TryValidateDeviceLoginChallengeToken(
        string token,
        CancellationToken ct,
        out Guid userId,
        out Guid verificationId,
        out bool twoFactorAlreadySatisfied)
    {
        userId = default;
        verificationId = default;
        twoFactorAlreadySatisfied = false;
        ct.ThrowIfCancellationRequested();
        var jwt = options.Value;
        var signingKey = CreateSigningKey(jwt.SigningKey);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtToken
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
            {
                return false;
            }

            var usage = principal.FindFirst(TokenUsageClaim)?.Value;
            if (!string.Equals(usage, DeviceLoginChallengeValue, StringComparison.Ordinal))
            {
                return false;
            }

            var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            var devVer = principal.FindFirst(DeviceVerificationIdClaim)?.Value;
            if (sub is null || devVer is null)
            {
                return false;
            }

            if (!Guid.TryParse(sub, out userId) || !Guid.TryParse(devVer, out verificationId))
            {
                return false;
            }

            var tf = principal.FindFirst(TwoFactorSatisfiedClaim)?.Value;
            twoFactorAlreadySatisfied = string.Equals(tf, "1", StringComparison.Ordinal);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
