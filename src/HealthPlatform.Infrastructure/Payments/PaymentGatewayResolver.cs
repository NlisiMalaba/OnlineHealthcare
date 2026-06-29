using HealthPlatform.Application.Payments;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class PaymentGatewayResolver(
    IEnumerable<IPaymentGateway> gateways,
    IOptions<PaymentGatewaysOptions> options) : IPaymentGatewayResolver
{
    private readonly IReadOnlyDictionary<string, IPaymentGateway> _gateways =
        gateways.ToDictionary(g => g.ProviderName, StringComparer.OrdinalIgnoreCase);

    public IPaymentGateway GetRequired(string providerName)
    {
        if (_gateways.TryGetValue(providerName, out var gateway))
        {
            return gateway;
        }

        throw new InvalidOperationException($"Payment gateway '{providerName}' is not registered.");
    }

    public IReadOnlyCollection<IPaymentGateway> GetAll() => _gateways.Values.ToList();

    public string ActiveProvider =>
        string.IsNullOrWhiteSpace(options.Value.ActiveProvider)
            ? PaymentGatewayProviders.Flutterwave
            : options.Value.ActiveProvider;
}
