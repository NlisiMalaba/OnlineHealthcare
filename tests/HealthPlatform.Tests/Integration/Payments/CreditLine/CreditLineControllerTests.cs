using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Payments.CreditLine;
using HealthPlatform.Application.Payments.CreditLine.GetCreditLine;
using HealthPlatform.Application.Payments.CreditLine.PayOnCreditLine;
using HealthPlatform.Domain.Appointments;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Payments.CreditLine;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Payments.CreditLine;

public sealed class CreditLineControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Pay_on_credit_line_records_balance_sends_reminder_and_confirms_appointment()
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
                "Credit Patient",
                null,
                $"credit-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var creditLine = PatientCreditLine.Open(patient.Id, 10_000, 720m, "USD");
        await _host.GetRequiredService<IPatientCreditLineRepository>().AddAsync(creditLine, CancellationToken.None);
        await _host.DbContext.SaveChangesAsync();

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                DateTime.UtcNow.AddHours(6)),
            CancellationToken.None);

        _host.CreditRepaymentReminderNotifier.Notifications.Clear();
        _host.CreditBalanceWarningNotifier.Notifications.Clear();

        var payment = await _host.Sender.Send(
            new PayOnCreditLineCommand(
                8500,
                "USD",
                booking.AppointmentId,
                null,
                null),
            CancellationToken.None);

        Assert.Equal(8500, payment.OutstandingBalanceMinorUnits);
        Assert.True(payment.BalanceWarningEmitted);
        Assert.Single(_host.CreditRepaymentReminderNotifier.Notifications);
        Assert.Single(_host.CreditBalanceWarningNotifier.Notifications);

        var appointment = await _host.DbContext.Appointments.SingleAsync(a => a.Id == booking.AppointmentId);
        Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);

        var summary = await _host.Sender.Send(new GetCreditLineQuery(), CancellationToken.None);
        Assert.Equal(8500, summary.OutstandingBalanceMinorUnits);
        Assert.Equal(1500, summary.AvailableCreditMinorUnits);
    }

    [Fact]
    public async Task Pay_on_credit_line_rejects_charge_above_available_limit()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Limited Credit Patient",
                null,
                $"limited-credit-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var creditLine = PatientCreditLine.Open(patient.Id, 5000, 600m, "USD");
        await _host.GetRequiredService<IPatientCreditLineRepository>().AddAsync(creditLine, CancellationToken.None);
        await _host.DbContext.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new PayOnCreditLineCommand(
                6000,
                "USD",
                Guid.CreateVersion7(),
                null,
                null),
            CancellationToken.None));

        Assert.Equal(CreditLineErrorCodes.CreditLimitExceeded, ex.Code);
    }
}
