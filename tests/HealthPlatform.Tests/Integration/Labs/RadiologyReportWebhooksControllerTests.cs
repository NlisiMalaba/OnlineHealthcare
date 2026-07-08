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

public sealed class RadiologyReportWebhooksControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task IngestAsync_stores_report_and_imaging_and_attaches_to_health_record()
    {
        var patientRegistration = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Radiology Patient",
                null,
                $"radiology-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);

        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == patientRegistration.PatientId);
        var doctorRegistration = await _host.Sender.Send(DoctorRegistrationTestData.CreateValidCommand(), CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId), CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        var labOrder = LabOrder.CreateDoctorOrdered(
            patient.Id,
            patientRegistration.HealthRecordId,
            doctor.Id,
            "LABX",
            "XRAY_CHEST",
            "Radiology order",
            DateTime.UtcNow);
        labOrder.MarkSubmitted("LABX-RAD-001");
        await _host.GetRequiredService<ILabOrderRepository>().AddAsync(labOrder, CancellationToken.None);
        await _host.GetRequiredService<ILabOrderRepository>().SaveChangesAsync(CancellationToken.None);

        var report = new FormFile(new MemoryStream([0x25, 0x50, 0x44, 0x46]), 0, 4, "report", "report.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };
        var imageOne = new FormFile(new MemoryStream([0x89, 0x50, 0x4E, 0x47]), 0, 4, "img1", "image1.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };
        var imageTwo = new FormFile(new MemoryStream([0x89, 0x50, 0x4E, 0x47]), 0, 4, "img2", "image2.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var controller = new RadiologyReportWebhooksController(_host.Sender);
        var action = await controller.IngestAsync(
            "LABX",
            new IngestRadiologyReportWebhookRequest("LABX-RAD-001", report, [imageOne, imageTwo]),
            CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(action.Result);
        var result = Assert.IsType<IngestRadiologyReportWebhookResultDto>(ok.Value);
        Assert.True(result.Accepted);
        Assert.Equal(2, result.ImagingFileCount);

        var storedReport = await _host.DbContext.RadiologyReports.SingleAsync(x => x.Id == result.RadiologyReportId);
        Assert.Equal(labOrder.Id, storedReport.LabOrderId);
        Assert.Contains("/radiology/reports/", storedReport.ReportStorageKey, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("/radiology/images/", storedReport.ImagingStorageKeysJson, StringComparison.OrdinalIgnoreCase);

        var entry = _host.HealthRecordEntryRepository.Entries.SingleOrDefault(e =>
            e.HealthRecordId == patientRegistration.HealthRecordId
            && e.EntryType == HealthRecordEntryType.RadiologyReportRef);
        Assert.NotNull(entry);
        Assert.Equal(result.RadiologyReportId, entry!.Content.RadiologyReportRef!.RadiologyReportId);
    }
}
