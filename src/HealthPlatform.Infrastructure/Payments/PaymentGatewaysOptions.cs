using HealthPlatform.Application.Payments;

namespace HealthPlatform.Infrastructure.Payments;

public sealed class PaymentGatewaysOptions
{
    public const string SectionName = "Payments:Gateways";

    public string ActiveProvider { get; set; } = PaymentGatewayProviders.Flutterwave;

    public StripeGatewayOptions Stripe { get; set; } = new();

    public FlutterwaveGatewayOptions Flutterwave { get; set; } = new();

    public PaystackGatewayOptions Paystack { get; set; } = new();

    public MpesaGatewayOptions Mpesa { get; set; } = new();
}

public abstract class GatewayOptionsBase
{
    public bool Enabled { get; set; }

    public string? SecretKey { get; set; }

    public string? WebhookSecret { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}

public sealed class StripeGatewayOptions : GatewayOptionsBase;

public sealed class FlutterwaveGatewayOptions : GatewayOptionsBase
{
    public string? PublicKey { get; set; }
}

public sealed class PaystackGatewayOptions : GatewayOptionsBase;

public sealed class MpesaGatewayOptions : GatewayOptionsBase
{
    public string? ConsumerKey { get; set; }

    public string? ConsumerSecret { get; set; }

    public string? Passkey { get; set; }

    public string? ShortCode { get; set; }

    public string? CallbackUrl { get; set; }
}
