using FsCheck;
using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Tests.Arbitraries;

public enum PaymentFailureTargetKind
{
    Appointment = 0,
    MedicationOrder = 1
}

public sealed record PaymentFailureCase(
    PaymentFailureTargetKind TargetKind,
    long AmountMinorUnits,
    string FailureCode,
    string FailureMessage);

public static class PaymentFailureArbitraries
{
    private static readonly string[] FailureCodes = ["card_declined", "insufficient_funds", "expired_card", "processing_error"];

    public static Arbitrary<PaymentFailureCase> PaymentFailureCase() =>
        (from target in Gen.Elements(PaymentFailureTargetKind.Appointment, PaymentFailureTargetKind.MedicationOrder)
         from amount in Gen.Choose(100, 500_000).Select(i => (long)i)
         from code in Gen.Elements(FailureCodes)
         from message in Gen.Elements(
             "Your card was declined.",
             "Insufficient funds on the payment method.",
             "The payment session expired before completion.",
             "The payment processor reported a temporary error.")
         select new PaymentFailureCase(target, amount, code, message))
        .ToArbitrary();
}
