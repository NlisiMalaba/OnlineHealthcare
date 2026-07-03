using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.ListPatientHealthRecordEntries;

public sealed class ListPatientHealthRecordEntriesQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    IHealthRecordAccessAuditService healthRecordAccessAuditService)
    : IRequestHandler<ListPatientHealthRecordEntriesQuery, IReadOnlyList<HealthRecordEntryDto>>
{
    public async Task<IReadOnlyList<HealthRecordEntryDto>> Handle(
        ListPatientHealthRecordEntriesQuery request,
        CancellationToken ct)
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

        await healthRecordAccessAuditService.LogPatientAccessAsync(
            patient.Id,
            healthRecord.Id,
            HealthRecordAccessOperations.ListPatientEntries,
            ct);

        return entries;
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
