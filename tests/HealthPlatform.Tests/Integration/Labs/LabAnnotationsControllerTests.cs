using HealthPlatform.API.Controllers;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;
using HealthPlatform.Application.Identity.RegisterPatient;
using HealthPlatform.Application.Identity.VerifyDoctorLicense;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.AnnotateDiagnosticReport;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Labs;
using HealthPlatform.Tests.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HealthPlatform.Tests.Integration.Labs;

public sealed class LabAnnotationsControllerTests : IAsyncLifetime
{
    private PatientRegistrationTestHost _host = null!;

    public async Task InitializeAsync()
    {
        _host = new PatientRegistrationTestHost();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await _host.DisposeAsync();

    [Fact]
    public async Task CreateAsync_creates_patient_visible_annotation_for_lab_result()
    {
        var patientRegistration = await _host.Sender.Send(
            new RegisterPatientCommand(
                PatientAuthProvider.Email,
                "Annotation Patient",
                null,
                $"annotation-{Guid.NewGuid():N}@example.com",
                PatientRegistrationTestHost.ValidPassword,
                null),
            CancellationToken.None);
        var patient = await _host.DbContext.Patients.SingleAsync(p => p.Id == patientRegistration.PatientId);

        var doctorRegistration = await _host.Sender.Send(DoctorRegistrationTestData.CreateValidCommand(), CancellationToken.None);
        await _host.Sender.Send(new VerifyDoctorLicenseCommand(doctorRegistration.DoctorId), CancellationToken.None);
        var doctor = await _host.DbContext.Doctors.SingleAsync(d => d.Id == doctorRegistration.DoctorId);

        var labResult = LabResult.Create(
            Guid.CreateVersion7(),
            patient.Id,
            patientRegistration.HealthRecordId,
            doctor.Id,
            "LABX",
            "LABX-ANN-001",
            "CBC",
            "patients/x/lab-result.pdf",
            "application/pdf",
            "lab-result.pdf",
            false);
        await _host.GetRequiredService<ILabResultRepository>().AddAsync(labResult, CancellationToken.None);
        await _host.GetRequiredService<ILabResultRepository>().SaveChangesAsync(CancellationToken.None);

        _host.CurrentUser.UserId = patient.UserId;
        await _host.Sender.Send(
            new GrantHealthRecordAccessCommand(doctor.Id, HealthRecordAccessType.Full, null),
            CancellationToken.None);

        _host.CurrentUser.UserId = doctor.UserId;
        var controller = new LabAnnotationsController(_host.Sender);
        var action = await controller.CreateAsync(
            new CreateLabAnnotationRequest(
                DiagnosticAnnotationTargetType.LabResult,
                labResult.Id,
                "Findings reviewed with patient."),
            CancellationToken.None);

        var created = Assert.IsType<CreatedResult>(action.Result);
        var entry = Assert.IsType<HealthRecordEntryDto>(created.Value);
        Assert.Equal(HealthRecordEntryType.DiagnosticReportAnnotation, entry.EntryType);
        Assert.True(entry.IsVisibleToPatient);
        Assert.Equal(labResult.Id, entry.Content.DiagnosticReportAnnotation!.TargetId);
    }
}
