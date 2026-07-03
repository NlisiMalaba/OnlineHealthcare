using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.GrantHealthRecordAccess;

public sealed class GrantHealthRecordAccessCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordAccessRepository healthRecordAccessRepository,
    IHealthRecordAccessAuditService healthRecordAccessAuditService,
    TimeProvider timeProvider)
    : IRequestHandler<GrantHealthRecordAccessCommand, HealthRecordAccessDto>
{
    public async Task<HealthRecordAccessDto> Handle(GrantHealthRecordAccessCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordNotFound,
                "Health record was not found.");

        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId, ct)
            ?? throw new NotFoundException(
                HealthRecordErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        var grantedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var activeGrant = await healthRecordAccessRepository.GetActiveGrantAsync(
            healthRecord.Id,
            doctor.Id,
            ct);

        if (activeGrant is not null)
        {
            throw new ConflictException(
                HealthRecordErrorCodes.HealthRecordAccessAlreadyGranted,
                "Doctor already has active access to this health record.");
        }

        var latestGrant = await healthRecordAccessRepository.GetLatestGrantAsync(
            healthRecord.Id,
            doctor.Id,
            ct);

        HealthRecordAccess access;
        if (latestGrant is null)
        {
            access = HealthRecordAccess.Grant(
                healthRecord.Id,
                doctor.Id,
                request.AccessType,
                request.Sections,
                grantedAtUtc);
            await healthRecordAccessRepository.AddAsync(access, ct);
        }
        else
        {
            latestGrant.Reactivate(grantedAtUtc, request.AccessType, request.Sections);
            await healthRecordAccessRepository.UpdateAsync(latestGrant, ct);
            access = latestGrant;
        }

        await healthRecordAccessAuditService.LogGrantAsync(
            patient.Id,
            healthRecord.Id,
            doctor.Id,
            request.AccessType,
            ct);

        return access.ToDto(doctor.FullName);
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
