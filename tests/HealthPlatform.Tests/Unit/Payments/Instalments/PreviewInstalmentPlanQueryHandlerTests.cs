using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Application.Payments.Instalments.PreviewInstalmentPlan;
using HealthPlatform.Domain.Payments.Instalments;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthPlatform.Tests.Unit.Payments.Instalments;

public sealed class PreviewInstalmentPlanQueryHandlerTests
{
    [Fact]
    public async Task Preview_rejects_instalment_count_above_configured_maximum()
    {
        var handler = CreateHandler(maxInstalments: 6);

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new PreviewInstalmentPlanQuery(
                12_000,
                InstalmentFrequency.Monthly,
                8,
                "USD",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))),
            CancellationToken.None));

        Assert.Equal(InstalmentErrorCodes.InvalidInstalmentPlan, ex.Code);
    }

    [Fact]
    public async Task Preview_rejects_expense_below_minimum_threshold()
    {
        var handler = CreateHandler(minimumExpenseMinorUnits: 10_000);

        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.Handle(
            new PreviewInstalmentPlanQuery(
                9_999,
                InstalmentFrequency.Weekly,
                2,
                "USD",
                DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))),
            CancellationToken.None));

        Assert.Equal(InstalmentErrorCodes.ExpenseBelowThreshold, ex.Code);
    }

    [Fact]
    public async Task Preview_returns_schedule_with_equal_instalments_when_total_divides_evenly()
    {
        var handler = CreateHandler();

        var preview = await handler.Handle(
            new PreviewInstalmentPlanQuery(
                12_000,
                InstalmentFrequency.Biweekly,
                4,
                "USD",
                new DateOnly(2026, 8, 1)),
            CancellationToken.None);

        Assert.Equal(3000, preview.InstalmentAmountMinorUnits);
        Assert.Equal(12_000, preview.TotalRepayableMinorUnits);
        Assert.Equal(4, preview.Schedule.Count);
        Assert.Equal(new DateOnly(2026, 8, 15), preview.Schedule[1].DueDate);
    }

    private static PreviewInstalmentPlanQueryHandler CreateHandler(
        long minimumExpenseMinorUnits = 10_000,
        int maxInstalments = 12) =>
        new(Options.Create(new InstalmentPlanOptions
        {
            MinimumExpenseMinorUnits = minimumExpenseMinorUnits,
            MaxInstalments = maxInstalments
        }));
}
