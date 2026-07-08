using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Labs;
using MediatR;

namespace HealthPlatform.Application.Labs.Webhooks;

public sealed class IngestRadiologyReportWebhookCommandHandler(
    ILabOrderRepository labOrderRepository,
    IRadiologyReportRepository radiologyReportRepository,
    IStorageService storageService,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<IngestRadiologyReportWebhookCommand, IngestRadiologyReportWebhookResultDto>
{
    public async Task<IngestRadiologyReportWebhookResultDto> Handle(
        IngestRadiologyReportWebhookCommand request,
        CancellationToken ct)
    {
        var order = await labOrderRepository.GetByPartnerReferenceAsync(
            request.LabPartnerCode,
            request.LabPartnerOrderReference,
            ct);
        if (order is null)
        {
            throw new NotFoundException(
                LabOrderErrorCodes.LabOrderReferenceNotFound,
                "Lab order matching the provided partner reference was not found.");
        }

        await using var reportStream = new MemoryStream(request.ReportFileContent, writable: false);
        var reportUpload = await storageService.UploadRadiologyReportAsync(
            order.PatientId,
            order.Id,
            reportStream,
            request.ReportContentType,
            request.ReportFileName,
            ct);

        var imagingStorageKeys = new List<string>(request.ImagingFiles.Count);
        foreach (var imagingFile in request.ImagingFiles)
        {
            await using var imagingStream = new MemoryStream(imagingFile.FileContent, writable: false);
            var upload = await storageService.UploadRadiologyImagingFileAsync(
                order.PatientId,
                order.Id,
                imagingStream,
                imagingFile.ContentType,
                imagingFile.FileName,
                ct);
            imagingStorageKeys.Add(upload.StorageKey);
        }

        var radiologyReport = RadiologyReport.Create(
            order.Id,
            order.PatientId,
            order.HealthRecordId,
            order.OrderingDoctorId,
            order.LabPartnerCode,
            order.LabPartnerOrderReference ?? request.LabPartnerOrderReference,
            reportUpload.StorageKey,
            reportUpload.ContentType,
            request.ReportFileName,
            imagingStorageKeys);

        await radiologyReportRepository.AddAsync(radiologyReport, ct);
        await healthRecordEntryRepository.AddAsync(
            new HealthRecordEntryCreateModel(
                order.HealthRecordId,
                HealthRecordEntryType.RadiologyReportRef,
                new HealthRecordEntryContentPayload(
                    RadiologyReportRef: new RadiologyReportRefContent(radiologyReport.Id)),
                order.OrderingDoctorId ?? Guid.Empty,
                timeProvider.GetUtcNow().UtcDateTime,
                true),
            ct);

        await radiologyReportRepository.SaveChangesAsync(ct);
        return new IngestRadiologyReportWebhookResultDto(
            true,
            radiologyReport.Id,
            order.Id,
            reportUpload.StorageKey,
            imagingStorageKeys.Count);
    }
}
