using System.Text;
using HealthPlatform.API.Controllers;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Insurance.SubmitInsuranceClaim;
using HealthPlatform.Application.Insurance.Webhooks;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Insurance;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Insurance;

public sealed class InsuranceClaimWebhooksControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task Insurer_webhook_updates_claim_status_for_patient()
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
                "Webhook Insurance Patient",
                null,
                $"insurance-webhook-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync();
        _host.CurrentUser.UserId = patient.UserId;

        var policy = PatientInsurancePolicy.Create(
            patient.Id,
            "demo-insurer",
            "POL-789",
            null,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-1)),
            null);

        await _host.GetRequiredService<IPatientInsurancePolicyRepository>().AddAsync(policy, CancellationToken.None);

        var booking = await _host.Sender.Send(
            new HealthPlatform.Application.Appointments.BookAppointment.BookAppointmentCommand(
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

        var controller = new InsuranceClaimWebhooksController(_host.Sender)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var rawBody = $$"""
            {
              "event_id": "evt-1",
              "claim_reference": "{{submitted.InsurerClaimReference}}",
              "status": "Approved",
              "status_reason": "Covered under plan"
            }
            """;

        controller.ControllerContext.HttpContext.Request.Headers["x-insurer-signature"] = "dev:test";
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(rawBody));

        var actionResult = await controller.ProcessAsync("demo-insurer", CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var payload = Assert.IsType<ProcessInsuranceClaimWebhookResultDto>(ok.Value);

        Assert.True(payload.Accepted);
        Assert.Equal(InsuranceClaimStatus.Approved, payload.Status);

        var claim = await _host.DbContext.InsuranceClaims.SingleAsync(c => c.Id == submitted.Id);
        Assert.Equal(InsuranceClaimStatus.Approved, claim.Status);
        Assert.Equal("Covered under plan", claim.StatusReason);
    }
}
