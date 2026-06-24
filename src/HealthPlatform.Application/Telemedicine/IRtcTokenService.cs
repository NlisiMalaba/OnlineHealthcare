using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine;

public sealed record RtcTokenRequest(
    string ChannelName,
    uint Uid,
    RtcProvider Provider,
    TimeSpan Ttl);

public sealed record RtcTokenResult(
    string Token,
    DateTime ExpiresAtUtc);

public interface IRtcTokenService
{
    Task<RtcTokenResult> GenerateTokenAsync(RtcTokenRequest request, CancellationToken ct);
}
