using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.API.Requests.Telemedicine;

public sealed class JoinTelemedicineSessionRequest
{
    public TelemedicineSessionMode? Mode { get; init; }
}
