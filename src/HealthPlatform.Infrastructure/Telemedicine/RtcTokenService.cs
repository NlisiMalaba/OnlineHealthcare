using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Domain.Telemedicine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HealthPlatform.Infrastructure.Telemedicine;

public sealed class RtcTokenService(
    IOptions<RtcOptions> options,
    ILogger<RtcTokenService> logger) : IRtcTokenService
{
    public Task<RtcTokenResult> GenerateTokenAsync(RtcTokenRequest request, CancellationToken ct)
    {
        var settings = options.Value;
        var expiresAtUtc = DateTime.UtcNow.Add(request.Ttl);
        var expireTimestamp = (uint)new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds();

        var token = request.Provider switch
        {
            RtcProvider.Agora when HasAgoraCredentials(settings) =>
                Agora.AgoraAccessTokenBuilder.Build(
                    settings.AgoraAppId!,
                    settings.AgoraAppCertificate!,
                    request.ChannelName,
                    request.Uid,
                    expireTimestamp),
            RtcProvider.Twilio when HasTwilioCredentials(settings) =>
                BuildTwilioToken(settings, request.ChannelName, request.Uid, expiresAtUtc),
            _ => BuildDevelopmentToken(request, expiresAtUtc)
        };

        if (token.StartsWith("dev:", StringComparison.Ordinal))
        {
            logger.LogWarning(
                "Generated development RTC token for channel {ChannelName}. Configure Rtc credentials for production.",
                request.ChannelName);
        }

        return Task.FromResult(new RtcTokenResult(token, expiresAtUtc));
    }

    private static bool HasAgoraCredentials(RtcOptions settings) =>
        !string.IsNullOrWhiteSpace(settings.AgoraAppId)
        && !string.IsNullOrWhiteSpace(settings.AgoraAppCertificate);

    private static bool HasTwilioCredentials(RtcOptions settings) =>
        !string.IsNullOrWhiteSpace(settings.TwilioAccountSid)
        && !string.IsNullOrWhiteSpace(settings.TwilioApiKeySid)
        && !string.IsNullOrWhiteSpace(settings.TwilioApiKeySecret);

    private static string BuildTwilioToken(
        RtcOptions settings,
        string channelName,
        uint uid,
        DateTime expiresAtUtc)
    {
        var identity = uid.ToString();
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.TwilioApiKeySecret!)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: settings.TwilioApiKeySid,
            audience: settings.TwilioAccountSid,
            claims:
            [
                new Claim("grants", $"{{\"video\":{{\"room\":\"{channelName}\"}}}}"),
                new Claim("identity", identity)
            ],
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string BuildDevelopmentToken(RtcTokenRequest request, DateTime expiresAtUtc) =>
        $"dev:{request.Provider}:{request.ChannelName}:{request.Uid}:{expiresAtUtc:O}";
}
