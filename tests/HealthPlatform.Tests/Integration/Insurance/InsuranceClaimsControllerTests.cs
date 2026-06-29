using HealthPlatform.Application.Appointments.BookAppointment;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Insurance.GetInsuranceClaim;
using HealthPlatform.Application.Insurance.ListPatientInsuranceClaims;
using HealthPlatform.Application.Insurance.SubmitInsuranceClaim;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Insurance;
using HealthPlatform.Tests.Support;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Insurance;

public sealed class InsuranceClaimsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Submit_claim_transmits_to_insurer_and_patient_can_read_status()
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
                "Insurance Patient",
                null,
                $"insurance-patient-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var policy = PatientInsurancePolicy.Create(
            patient.Id,
            "demo-insurer",
            "POL-123",
            "MEM-456",
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)));

        await _host.GetRequiredService<IPatientInsurancePolicyRepository>().AddAsync(policy, CancellationToken.None);

        var booking = await _host.Sender.Send(
            new BookAppointmentCommand(
                doctor.Id,
                doctor.AvailabilitySlots.First().Id,
                DateTime.UtcNow.AddHours(6)),
            CancellationToken.None);

        var submitted = await _host.Sender.Send(
            new SubmitInsuranceClaimCommand(
                "demo-insurer",
                InsuranceClaimType.Consultation,
                2500,
                "USD",
                booking.AppointmentId,
                null,
                null),
            CancellationToken.None);

        Assert.Equal(InsuranceClaimStatus.Submitted, submitted.Status);
        Assert.StartsWith("dev_demo-insurer_", submitted.InsurerClaimReference);

        var fetched = await _host.Sender.Send(
            new GetInsuranceClaimQuery(submitted.Id),
            CancellationToken.None);

        Assert.Equal(submitted.Id, fetched.Id);
        Assert.Equal(InsuranceClaimStatus.Submitted, fetched.Status);

        var list = await _host.Sender.Send(new ListPatientInsuranceClaimsQuery(), CancellationToken.None);
        Assert.Contains(list, item => item.Id == submitted.Id);
    }

    [Fact]
    public async Task Submit_claim_rejects_patient_without_active_policy()
    {
        await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Uninsured Patient",
                null,
                $"uninsured-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var ex = await Assert.ThrowsAsync<DomainException>(() => _host.Sender.Send(
            new SubmitInsuranceClaimCommand(
                "demo-insurer",
                InsuranceClaimType.Consultation,
                2500,
                "USD",
                Guid.CreateVersion7(),
                null,
                null),
            CancellationToken.None));

        Assert.Equal(InsuranceErrorCodes.PolicyInactive, ex.Code);
    }
}
