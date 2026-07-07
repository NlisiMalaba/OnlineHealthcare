using HealthPlatform.API.Controllers;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.Webhooks;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Labs;

public sealed class LabResultWebhooksControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task IngestAsync_stores_result_attaches_health_record_entry_and_notifies_parties()
    {
        var patientRegistration = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Lab Result Patient",
                null,
                $"lab-result-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == patientRegistration.PatientId);
        var doctorRegistration = await _host.Sender.Send(
            DoctorRegistrationTestData.CreateValidCommand(),
            CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId), CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        var labOrder = LabOrder.CreateDoctorOrdered(
            patient.Id,
            patientRegistration.HealthRecordId,
            doctor.Id,
            "LABX",
            "CBC",
            "Routine panel",
            DateTime.UtcNow);
        labOrder.MarkSubmitted("LABX-ORDER-1001");
        await _host.GetRequiredService<ILabOrderRepository>().AddAsync(labOrder, CancellationToken.None);
        await _host.GetRequiredService<ILabOrderRepository>().SaveChangesAsync(CancellationToken.None);

        var payload = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        var formFile = new FormFile(new MemoryStream(payload), 0, payload.Length, "result", "cbc-result.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var controller = new LabResultWebhooksController(_host.Sender);
        var action = await controller.IngestAsync(
            "LABX",
            new IngestLabResultWebhookRequest("LABX-ORDER-1001", "CBC", false, formFile),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var result = Assert.IsType<IngestLabResultWebhookResultDto>(ok.Value);
        Assert.True(result.Accepted);
        Assert.Equal(labOrder.Id, result.LabOrderId);

        var persisted = await _host.DbContext.LabResults.SingleAsync(r => r.Id == result.LabResultId);
        Assert.Equal(labOrder.Id, persisted.LabOrderId);
        Assert.Equal("CBC", persisted.TestCode);
        Assert.EndsWith(".pdf", persisted.StorageKey, StringComparison.OrdinalIgnoreCase);

        var labEntry = _host.HealthRecordEntryRepository.Entries
            .SingleOrDefault(e =>
                e.HealthRecordId == patientRegistration.HealthRecordId
                && e.EntryType == HealthRecordEntryType.LabResultRef);
        Assert.NotNull(labEntry);
        Assert.Equal(persisted.Id, labEntry!.Content.LabResultRef!.LabResultId);

        var notificationLogs = await _host.DbContext.NotificationLogs
            .Where(log => log.EventType == HealthPlatform.Application.Notifications.NotificationEventTypes.LabResultUploaded)
            .ToListAsync();
        Assert.Contains(notificationLogs, log => log.RecipientId == patient.UserId);
        Assert.Contains(notificationLogs, log => log.RecipientId == doctor.UserId);
    }
}
