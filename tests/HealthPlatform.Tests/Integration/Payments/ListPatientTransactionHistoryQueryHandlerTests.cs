using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments;
using HealthPlatform.Application.Payments.CreditLine.PayOnCreditLine;
using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Application.Payments.ListPatientTransactionHistory;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Payments;

public sealed class ListPatientTransactionHistoryQueryHandlerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Transaction_history_includes_completed_payments_failed_payments_and_credit_line_charges()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "History Patient",
                null,
                $"history-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var completionService = _host.GetRequiredService<IPaymentCompletionService>();
        var completed = await completionService.CompleteAsync(
            new CompletePaymentRequest(
                patient.Id,
                3200,
                "USD",
                PaymentMethod.Card,
                PaymentGatewayType.Paystack,
                "ref_completed",
                null,
                Guid.CreateVersion7(),
                null,
                DateTime.UtcNow.AddMinutes(-5)),
            CancellationToken.None);

        var failureService = _host.GetRequiredService<IPaymentFailureService>();
        var failed = await failureService.RecordFailureAsync(
            new RecordPaymentFailureRequest(
                patient.Id,
                1800,
                "USD",
                PaymentMethod.MobileMoney,
                PaymentGatewayType.Mpesa,
                "ref_failed",
                null,
                Guid.CreateVersion7(),
                null,
                "processing_error",
                "Payment could not be processed.",
                DateTime.UtcNow.AddMinutes(-2)),
            CancellationToken.None);

        var creditLine = PatientCreditLine.Open(patient.Id, 20_000, 700m, "USD");
        await _host.GetRequiredService<IPatientCreditLineRepository>().AddAsync(creditLine, CancellationToken.None);
        await _host.DbContext.SaveChangesAsync();

        await _host.Sender.Send(
            new PayOnCreditLineCommand(
                2500,
                "USD",
                null,
                Guid.CreateVersion7(),
                null),
            CancellationToken.None);

        var history = await _host.Sender.Send(new ListPatientTransactionHistoryQuery(), CancellationToken.None);

        var completedEntry = history.Single(item => item.PaymentId == completed.PaymentId);
        Assert.Equal(PatientTransactionCategory.MedicationCost, completedEntry.Category);
        Assert.Equal("Completed", completedEntry.Status);
        Assert.NotNull(completedEntry.ReceiptUrl);

        var failedEntry = history.Single(item => item.EntryId == failed.PaymentId);
        Assert.Equal("Failed", failedEntry.Status);
        Assert.Null(failedEntry.ReceiptUrl);

        Assert.Contains(history, item =>
            item.Category == PatientTransactionCategory.CreditLine
            && item.AmountMinorUnits == 2500);

        Assert.True(history[0].OccurredAtUtc >= history[^1].OccurredAtUtc);
    }
}
