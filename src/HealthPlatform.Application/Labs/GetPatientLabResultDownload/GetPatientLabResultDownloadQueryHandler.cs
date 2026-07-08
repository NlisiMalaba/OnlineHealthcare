using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.Labs.GetPatientLabResultDownload;

public sealed class GetPatientLabResultDownloadQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    ILabResultRepository labResultRepository,
    IStorageService storageService,
    IHealthRecordAccessAuditService healthRecordAccessAuditService)
    : IRequestHandler<GetPatientLabResultDownloadQuery, LabResultDownloadDto>
{
    public async Task<LabResultDownloadDto> Handle(GetPatientLabResultDownloadQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var result = await labResultRepository.GetByIdAsync(request.LabResultId, ct)
            ?? throw new NotFoundException(
                LabOrderErrorCodes.LabResultNotFound,
                "Lab result was not found.");

        if (result.PatientId != patient.Id)
        {
            throw new NotFoundException(
                LabOrderErrorCodes.LabResultNotFound,
                "Lab result was not found.");
        }

        var downloadUrl = await storageService.GetSignedReadUrlAsync(result.StorageKey, ct);

        await healthRecordAccessAuditService.LogPatientAccessAsync(
            patient.Id,
            result.HealthRecordId,
            HealthRecordAccessOperations.DownloadLabResult,
            ct);

        return new LabResultDownloadDto(
            result.Id,
            result.FileName,
            result.ContentType,
            downloadUrl);
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(LabOrderErrorCodes.PatientNotFound, "Patient profile was not found.");
    }
}
