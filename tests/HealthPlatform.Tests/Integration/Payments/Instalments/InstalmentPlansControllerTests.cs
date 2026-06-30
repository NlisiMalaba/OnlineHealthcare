using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Application.Payments.Instalments.CreateInstalmentPlan;
using HealthPlatform.Application.Payments.Instalments.GetInstalmentPlan;
using HealthPlatform.Application.Payments.Instalments.PreviewInstalmentPlan;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments.Instalments;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Payments.Instalments;

public sealed class InstalmentPlansControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Preview_then_create_stores_scheduled_payments_and_confirms_appointment()
    {
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);

        var doctor = await _host.DbContext.Doctors
            .Include(d => d.AvailabilitySlots)
            .SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Instalment Patient",
                null,
                $"instalment-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                DateTime.UtcNow.AddHours(6)),
            CancellationToken.None);

        var firstDueDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));
        var preview = await _host.Sender.Send(
            new PreviewInstalmentPlanQuery(
                12_000,
                InstalmentFrequency.Monthly,
                3,
                "USD",
                firstDueDate),
            CancellationToken.None);

        Assert.Equal(4000, preview.InstalmentAmountMinorUnits);
        Assert.Equal(12_000, preview.TotalRepayableMinorUnits);
        Assert.Equal(3, preview.Schedule.Count);
        Assert.All(preview.Schedule, item => Assert.True(item.DueDate >= firstDueDate));

        var plan = await _host.Sender.Send(
            new CreateInstalmentPlanCommand(
                12_000,
                InstalmentFrequency.Monthly,
                3,
                "USD",
                firstDueDate,
                booking.AppointmentId,
                null,
                null),
            CancellationToken.None);

        Assert.Equal(3, plan.Schedule.Count);
        Assert.Equal(InstalmentPlanStatus.Active, plan.Status);

        var fetched = await _host.Sender.Send(new GetInstalmentPlanQuery(plan.Id), CancellationToken.None);
        Assert.Equal(plan.Id, fetched.Id);
        Assert.Equal(preview.TotalRepayableMinorUnits, fetched.TotalRepayableMinorUnits);

        var payments = await _host.DbContext.InstalmentPayments
            .Where(p => p.InstalmentPlanId == plan.Id)
            .ToListAsync();
        Assert.Equal(3, payments.Count);
        Assert.All(payments, payment => Assert.Equal(InstalmentPaymentStatus.Scheduled, payment.Status));

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == booking.AppointmentId);
        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
    }

    [Fact]
    public async Task Preview_rejects_expense_below_threshold()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Low Expense Patient",
                null,
                $"low-expense-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var ex = await Assert.ThrowsAsync<HealthPlatform.Application.Exceptions.DomainException>(() =>
            _host.Sender.Send(
                new PreviewInstalmentPlanQuery(
                    5000,
                    InstalmentFrequency.Weekly,
                    2,
                    "USD",
                    DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))),
                CancellationToken.None));

        Assert.Equal(InstalmentErrorCodes.ExpenseBelowThreshold, ex.Code);
    }
}
