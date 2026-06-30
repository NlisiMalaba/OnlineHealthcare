namespace HealthPlatform.Domain.Payments;

public enum PaymentGatewayType
{
    Stripe = 0,
    Flutterwave = 1,
    Paystack = 2,
    Mpesa = 3,
    Internal = 4
}
