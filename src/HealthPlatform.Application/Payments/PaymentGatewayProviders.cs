namespace HealthPlatform.Application.Payments;

public static class PaymentGatewayProviders
{
    public const string Stripe = "stripe";
    public const string Flutterwave = "flutterwave";
    public const string Paystack = "paystack";
    public const string Mpesa = "mpesa";

    public static readonly IReadOnlySet<string> All =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Stripe,
            Flutterwave,
            Paystack,
            Mpesa
        };
}
