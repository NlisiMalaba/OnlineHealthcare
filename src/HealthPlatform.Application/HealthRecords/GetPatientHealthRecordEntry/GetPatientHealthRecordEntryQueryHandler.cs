using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.GetPatientHealthRecordEntry;

public sealed class GetPatientHealthRecordEntryQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    IHealthRecordAccessAuditService healthRecordAccessAuditService)
    : IRequestHandler<GetPatientHealthRecordEntryQuery, HealthRecordEntryDto>
{
    public async Task<HealthRecordEntryDto> Handle(GetPatientHealthRecordEntryQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordNotFound,
                "Health record was not found.");

        var entry = await healthRecordEntryRepository.GetByIdAsync(request.EntryId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordEntryNotFound,
                "Health record entry was not found.");

        var allowed = entry.HealthRecordId == healthRecord.Id && entry.IsVisibleToPatient;
        await healthRecordAccessAuditService.LogPatientAccessAttemptAsync(
            patient.Id,
            healthRecord.Id,
            HealthRecordAccessOperations.GetPatientEntry,
            allowed,
            ct);

        if (!allowed)
        {
            throw new AccessDeniedException(
                "ACCESS_DENIED",
                "Patient cannot access this health record entry.");
        }

        return entry;
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
