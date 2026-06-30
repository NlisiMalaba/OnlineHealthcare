using HealthPlatform.Application.Payments;
using HealthPlatform.Tests.Support;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments;

public sealed class PaymentGatewayWebhookTests
{
    [Fact]
    public async Task Stripe_dev_webhook_parses_completed_payment_with_appointment_metadata()
    {
        var gateway = PaymentGatewayTestSupport.CreateStripeGateway();
        var appointmentId = Guid.CreateVersion7();
        var rawBody = $$"""
            {
              "id": "evt_test_123",
              "type": "payment_intent.succeeded",
              "data": {
                "object": {
                  "id": "pi_test_123",
                  "amount": 2500,
                  "currency": "usd",
                  "metadata": {
                    "appointment_id": "{{appointmentId}}"
                  }
                }
              }
            }
            """;

        var result = await gateway.ParseWebhookAsync(
            new PaymentWebhookParseRequestDto(
                rawBody,
                new Dictionary<string, string> { ["Stripe-Signature"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.SignatureValid);
        Assert.Equal("evt_test_123", result.EventId);
        Assert.Equal(PaymentWebhookEventStatus.Completed, result.Status);
        Assert.Equal(appointmentId, result.AppointmentId);
    }

    [Fact]
    public async Task Flutterwave_dev_webhook_parses_successful_charge()
    {
        var gateway = PaymentGatewayTestSupport.CreateFlutterwaveGateway();
        var appointmentId = Guid.CreateVersion7();
        var rawBody = $$"""
            {
              "event": "charge.completed",
              "data": {
                "id": 12345,
                "status": "successful",
                "amount": 25.00,
                "currency": "USD",
                "flw_ref": "FLW-REF-1",
                "meta": {
                  "appointment_id": "{{appointmentId}}"
                }
              }
            }
            """;

        var result = await gateway.ParseWebhookAsync(
            new PaymentWebhookParseRequestDto(
                rawBody,
                new Dictionary<string, string> { ["verif-hash"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.SignatureValid);
        Assert.Equal("12345", result.EventId);
        Assert.Equal(PaymentWebhookEventStatus.Completed, result.Status);
        Assert.Equal(2500, result.AmountMinorUnits);
        Assert.Equal(appointmentId, result.AppointmentId);
    }

    [Fact]
    public async Task Paystack_dev_webhook_parses_charge_success()
    {
        var gateway = PaymentGatewayTestSupport.CreatePaystackGateway();
        var appointmentId = Guid.CreateVersion7();
        var rawBody = $$"""
            {
              "event": "charge.success",
              "data": {
                "id": 98765,
                "reference": "ref_123",
                "amount": 5000,
                "currency": "NGN",
                "metadata": {
                  "appointment_id": "{{appointmentId}}"
                }
              }
            }
            """;

        var result = await gateway.ParseWebhookAsync(
            new PaymentWebhookParseRequestDto(
                rawBody,
                new Dictionary<string, string> { ["x-paystack-signature"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.SignatureValid);
        Assert.Equal("98765", result.EventId);
        Assert.Equal(PaymentWebhookEventStatus.Completed, result.Status);
        Assert.Equal(appointmentId, result.AppointmentId);
    }

    [Fact]
    public async Task Mpesa_dev_webhook_parses_successful_stk_callback()
    {
        var gateway = PaymentGatewayTestSupport.CreateMpesaGateway();
        var appointmentId = Guid.CreateVersion7();
        var rawBody = $$"""
            {
              "Body": {
                "stkCallback": {
                  "CheckoutRequestID": "ws_CO_123",
                  "ResultCode": 0,
                  "CallbackMetadata": {
                    "Item": [
                      { "Name": "Amount", "Value": 25.00 },
                      { "Name": "AccountReference", "Value": "{{appointmentId}}" }
                    ]
                  }
                }
              }
            }
            """;

        var result = await gateway.ParseWebhookAsync(
            new PaymentWebhookParseRequestDto(
                rawBody,
                new Dictionary<string, string> { ["x-dev-mpesa"] = "true" }),
            CancellationToken.None);

        Assert.True(result.SignatureValid);
        Assert.Equal("ws_CO_123", result.EventId);
        Assert.Equal(PaymentWebhookEventStatus.Completed, result.Status);
        Assert.Equal(2500, result.AmountMinorUnits);
        Assert.Equal(appointmentId, result.AppointmentId);
    }

    [Fact]
    public async Task Stripe_dev_webhook_parses_failed_payment_with_appointment_metadata()
    {
        var gateway = PaymentGatewayTestSupport.CreateStripeGateway();
        var appointmentId = Guid.CreateVersion7();
        var rawBody = $$"""
            {
              "id": "evt_failed_123",
              "type": "payment_intent.payment_failed",
              "data": {
                "object": {
                  "id": "pi_failed_123",
                  "amount": 2500,
                  "currency": "usd",
                  "metadata": {
                    "appointment_id": "{{appointmentId}}"
                  },
                  "last_payment_error": {
                    "code": "card_declined",
                    "message": "Your card was declined."
                  }
                }
              }
            }
            """;

        var result = await gateway.ParseWebhookAsync(
            new PaymentWebhookParseRequestDto(
                rawBody,
                new Dictionary<string, string> { ["Stripe-Signature"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.SignatureValid);
        Assert.Equal(PaymentWebhookEventStatus.Failed, result.Status);
        Assert.Equal(appointmentId, result.AppointmentId);
        Assert.Equal("card_declined", result.FailureCode);
        Assert.Equal("Your card was declined.", result.FailureMessage);
    }

    [Fact]
    public async Task Flutterwave_dev_webhook_parses_failed_charge_with_metadata()
    {
        var gateway = PaymentGatewayTestSupport.CreateFlutterwaveGateway();
        var medicationOrderId = Guid.CreateVersion7();
        var rawBody = $$"""
            {
              "event": "charge.completed",
              "data": {
                "id": 999,
                "status": "failed",
                "currency": "USD",
                "flw_ref": "FLW-FAILED",
                "meta": {
                  "medication_order_id": "{{medicationOrderId}}"
                }
              }
            }
            """;

        var result = await gateway.ParseWebhookAsync(
            new PaymentWebhookParseRequestDto(
                rawBody,
                new Dictionary<string, string> { ["verif-hash"] = "dev:test" }),
            CancellationToken.None);

        Assert.True(result.SignatureValid);
        Assert.Equal(PaymentWebhookEventStatus.Failed, result.Status);
        Assert.Equal(medicationOrderId, result.MedicationOrderId);
    }

    [Fact]
    public async Task Stripe_create_payment_intent_returns_development_reference_when_not_configured()
    {
        var gateway = PaymentGatewayTestSupport.CreateStripeGateway();

        var result = await gateway.CreatePaymentIntentAsync(
            new CreatePaymentIntentRequestDto(
                "USD",
                2500,
                "patient@example.com",
                "Consultation fee",
                "idem-123",
                new Dictionary<string, string> { [PaymentMetadataKeys.AppointmentId] = Guid.CreateVersion7().ToString() }),
            CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.StartsWith("dev_stripe_", result.ProviderPaymentId);
        Assert.StartsWith("dev_secret_", result.ClientSecret);
    }
}
