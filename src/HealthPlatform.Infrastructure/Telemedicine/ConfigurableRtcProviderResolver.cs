using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Domain.Telemedicine;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Telemedicine;

public sealed class ConfigurableRtcProviderResolver(IOptions<RtcOptions> options) : IRtcProviderResolver
{
    public RtcProvider Resolve() =>
        options.Value.Provider.Equals("Twilio", StringComparison.OrdinalIgnoreCase)
            ? RtcProvider.Twilio
            : RtcProvider.Agora;
}
