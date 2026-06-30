using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Application.Payments;

public static class PaymentGatewayMapper
{
    public static PaymentGatewayType FromProviderName(string providerName) =>
        providerName.ToLowerInvariant() switch
        {
            PaymentGatewayProviders.Stripe => PaymentGatewayType.Stripe,
            PaymentGatewayProviders.Flutterwave => PaymentGatewayType.Flutterwave,
            PaymentGatewayProviders.Paystack => PaymentGatewayType.Paystack,
            PaymentGatewayProviders.Mpesa => PaymentGatewayType.Mpesa,
            _ => PaymentGatewayType.Internal
        };

    public static PaymentMethod DefaultMethodForGateway(PaymentGatewayType gateway) =>
        gateway switch
        {
            PaymentGatewayType.Mpesa => PaymentMethod.MobileMoney,
            PaymentGatewayType.Internal => PaymentMethod.CreditLine,
            _ => PaymentMethod.Card
        };
}
