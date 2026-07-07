using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.ExportPatientHealthRecordPdf;

public sealed class ExportPatientHealthRecordPdfQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    IHealthRecordPdfGenerator healthRecordPdfGenerator,
    IHealthRecordAccessAuditService healthRecordAccessAuditService,
    IStorageService storageService,
    TimeProvider timeProvider)
    : IRequestHandler<ExportPatientHealthRecordPdfQuery, HealthRecordPdfExportDto>
{
    public async Task<HealthRecordPdfExportDto> Handle(ExportPatientHealthRecordPdfQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordNotFound,
                "Health record was not found.");

        var entries = await healthRecordEntryRepository.ListByHealthRecordIdAsync(
            healthRecord.Id,
            patientVisibleOnly: true,
            ct);

        var generatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var pdfBytes = healthRecordPdfGenerator.Generate(
            new PatientHealthRecordExportModel(
                healthRecord.Id,
                patient.Id,
                patient.FullName,
                generatedAtUtc,
                entries));

        await using var pdfStream = new MemoryStream(pdfBytes);
        var upload = await storageService.UploadHealthRecordExportAsync(
            patient.Id,
            healthRecord.Id,
            pdfStream,
            ct);

        var downloadUrl = await storageService.GetSignedReadUrlAsync(upload.StorageKey, ct);

        await healthRecordAccessAuditService.LogPatientAccessAsync(
            patient.Id,
            healthRecord.Id,
            HealthRecordAccessOperations.ExportPdf,
            ct);

        return new HealthRecordPdfExportDto(healthRecord.Id, downloadUrl, generatedAtUtc);
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
