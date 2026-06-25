using HealthPlatform.Domain.Telemedicine;

namespace HealthPlatform.Application.Telemedicine;

public interface IRtcProviderResolver
{
    RtcProvider Resolve();
}
