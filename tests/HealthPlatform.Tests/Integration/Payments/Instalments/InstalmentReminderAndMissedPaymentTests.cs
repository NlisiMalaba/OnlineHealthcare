using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments.Instalments;
using HealthPlatform.Infrastructure.Persistence.Repositories;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace HealthPlatform.Tests.Integration.Payments.Instalments;

public sealed class InstalmentReminderAndMissedPaymentTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Due_reminder_dispatcher_notifies_patient_24_hours_before_due_date()
    {
        var patient = Patient.RegisterWithEmail(
            Guid.CreateVersion7(),
            "Reminder Patient",
            $"reminder-{Guid.NewGuid():N}@example.com");
        _host.DbContext.Patients.Add(patient);
        await _host.DbContext.SaveChangesAsync();

        var plan = InstalmentPlan.Create(
            patient.Id,
            12_000,
            InstalmentFrequency.Monthly,
            2,
            "USD",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
            Guid.CreateVersion7(),
            null,
            null,
            10_000);

        var payment = InstalmentPayment.Schedule(
            plan.Id,
            patient.Id,
            new InstalmentScheduleEntry(1, 6000, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1))),
            "USD");

        _host.DbContext.InstalmentPlans.Add(plan);
        _host.DbContext.InstalmentPayments.Add(payment);
        await _host.DbContext.SaveChangesAsync();

        _host.InstalmentDueReminderNotifier.Notifications.Clear();

        var dispatcher = new InstalmentDueReminderDispatcher(
            new InstalmentPaymentRepository(_host.DbContext),
            _host.GetRequiredService<HealthPlatform.Application.Identity.IPatientRepository>(),
            _host.InstalmentDueReminderNotifier,
            TimeProvider.System,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<InstalmentDueReminderDispatcher>.Instance);

        var dispatched = await dispatcher.DispatchDueRemindersAsync(CancellationToken.None);

        Assert.Equal(1, dispatched);
        Assert.Single(_host.InstalmentDueReminderNotifier.Notifications);
    }

    [Fact]
    public async Task Missed_payment_processor_applies_late_fee_and_notifies_patient()
    {
        var patient = Patient.RegisterWithEmail(
            Guid.CreateVersion7(),
            "Missed Patient",
            $"missed-{Guid.NewGuid():N}@example.com");
        _host.DbContext.Patients.Add(patient);
        await _host.DbContext.SaveChangesAsync();

        var plan = InstalmentPlan.Create(
            patient.Id,
            12_000,
            InstalmentFrequency.Monthly,
            2,
            "USD",
            DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            Guid.CreateVersion7(),
            null,
            null,
            10_000);

        var payment = InstalmentPayment.Schedule(
            plan.Id,
            patient.Id,
            new InstalmentScheduleEntry(1, 6000, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3))),
            "USD");

        _host.DbContext.InstalmentPlans.Add(plan);
        _host.DbContext.InstalmentPayments.Add(payment);
        await _host.DbContext.SaveChangesAsync();

        _host.InstalmentMissedPaymentNotifier.Notifications.Clear();

        var processor = new InstalmentMissedPaymentProcessor(
            new InstalmentPaymentRepository(_host.DbContext),
            new InstalmentPlanRepository(_host.DbContext),
            _host.GetRequiredService<IOutboxRepository>(),
            _host.GetRequiredService<IDomainEventPublisher>(),
            Options.Create(new InstalmentPlanOptions { LateFeeMinorUnits = 750 }),
            TimeProvider.System,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<InstalmentMissedPaymentProcessor>.Instance);

        var processed = await processor.ProcessMissedPaymentsAsync(CancellationToken.None);

        Assert.Equal(1, processed);
        Assert.Single(_host.InstalmentMissedPaymentNotifier.Notifications);
        Assert.Equal(750, _host.InstalmentMissedPaymentNotifier.Notifications[0].LateFeeMinorUnits);

        var refreshedPlan = await _host.DbContext.InstalmentPlans.SingleAsync(p => p.Id == plan.Id);
        Assert.Equal(InstalmentPlanStatus.Defaulted, refreshedPlan.Status);
        Assert.Equal(12_750, refreshedPlan.TotalRepayableMinorUnits);

        var refreshedPayment = await _host.DbContext.InstalmentPayments.SingleAsync(p => p.Id == payment.Id);
        Assert.Equal(InstalmentPaymentStatus.Missed, refreshedPayment.Status);
    }
}
