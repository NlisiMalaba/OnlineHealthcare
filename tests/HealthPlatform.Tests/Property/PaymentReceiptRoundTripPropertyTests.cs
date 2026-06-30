using FsCheck.Xunit;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.GetPaymentReceipt;
using HealthPlatform.Application.Payments.ListPatientTransactionHistory;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Tests.Arbitraries;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace HealthPlatform.Tests.Properties;

public sealed class PaymentReceiptRoundTripPropertyTests
{
    // Feature: online-healthcare-platform, Property 16: Payment Receipt Round Trip
    [Property(Arbitrary = [typeof(PaymentReceiptArbitraries)], MaxTest = 100)]
    public bool Completed_payment_has_receipt_entry_in_transaction_history_referencing_payment_id(
        PaymentReceiptCase testCase) =>
        RunReceiptRoundTripInvariantAsync(testCase).GetAwaiter().GetResult();

    private static async Task<bool> RunReceiptRoundTripInvariantAsync(PaymentReceiptCase testCase)
    {
        await using var host = new PatientRegistrationTestHost();

        await host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Receipt Property Patient",
                null,
                $"receipt-property-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await host.DbContext.Patients.SingleAsync();
        host.CurrentUser.UserId = patient.UserId;

        var completedAtUtc = DateTime.UtcNow;
        var completion = host.GetRequiredService<IPaymentCompletionService>();
        var result = await completion.CompleteAsync(
            new CompletePaymentRequest(
                patient.Id,
                testCase.AmountMinorUnits,
                testCase.Currency,
                testCase.PaymentMethod,
                testCase.Gateway,
                $"ref_{Guid.NewGuid():N}",
                null,
                testCase.MedicationOrderId,
                testCase.LabOrderId,
                completedAtUtc),
            CancellationToken.None);

        var storedPayment = await host.DbContext.Payments
            .AsNoTracking()
            .SingleOrDefaultAsync(payment => payment.Id == result.PaymentId);

        if (storedPayment is null
            || string.IsNullOrWhiteSpace(storedPayment.ReceiptStorageKey)
            || storedPayment.AmountMinorUnits != testCase.AmountMinorUnits
            || storedPayment.Status != PaymentStatus.Completed)
        {
            return false;
        }

        var history = await host.Sender.Send(new ListPatientTransactionHistoryQuery(), CancellationToken.None);
        var historyEntry = history.SingleOrDefault(item => item.PaymentId == result.PaymentId);
        if (historyEntry is null
            || historyEntry.EntryId != result.PaymentId
            || historyEntry.AmountMinorUnits != testCase.AmountMinorUnits
            || string.IsNullOrWhiteSpace(historyEntry.ReceiptUrl))
        {
            return false;
        }

        var receipt = await host.Sender.Send(new GetPaymentReceiptQuery(result.PaymentId), CancellationToken.None);
        return receipt.PaymentId == result.PaymentId
            && !string.IsNullOrWhiteSpace(receipt.ReceiptReadUrl);
    }
}
