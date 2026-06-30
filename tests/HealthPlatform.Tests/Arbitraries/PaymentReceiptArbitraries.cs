using FsCheck;
using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Tests.Arbitraries;

public sealed record PaymentReceiptCase(
    long AmountMinorUnits,
    string Currency,
    PaymentMethod PaymentMethod,
    PaymentGatewayType Gateway,
    Guid? MedicationOrderId,
    Guid? LabOrderId);

public static class PaymentReceiptArbitraries
{
    private static readonly string[] SupportedCurrencies = ["USD", "KES", "NGN", "GBP"];

    public static Arbitrary<PaymentReceiptCase> PaymentReceiptCase() =>
        (from amount in Gen.Choose(1, 5_000_000).Select(i => (long)i)
         from currency in Gen.Elements(SupportedCurrencies)
         from method in Gen.Elements(
             PaymentMethod.Card,
             PaymentMethod.BankTransfer,
             PaymentMethod.MobileMoney,
             PaymentMethod.CreditLine,
             PaymentMethod.Instalment)
         from gateway in Gen.Elements(
             PaymentGatewayType.Stripe,
             PaymentGatewayType.Flutterwave,
             PaymentGatewayType.Paystack,
             PaymentGatewayType.Mpesa,
             PaymentGatewayType.Internal)
         from target in Gen.Elements(0, 1, 2)
         let medicationOrderId = target == 1 ? Guid.CreateVersion7() : (Guid?)null
         let labOrderId = target == 2 ? Guid.CreateVersion7() : (Guid?)null
         select new PaymentReceiptCase(amount, currency, method, gateway, medicationOrderId, labOrderId))
        .ToArbitrary();
}
