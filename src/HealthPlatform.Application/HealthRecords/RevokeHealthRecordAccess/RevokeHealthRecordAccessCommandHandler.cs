using System.Text.Json;
using HealthPlatform.Application.Audit;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Audit;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.HealthRecords.RevokeHealthRecordAccess;

public sealed class RevokeHealthRecordAccessCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordAccessRepository healthRecordAccessRepository,
    IAuditLogRepository auditLogRepository,
    IAuditContextAccessor auditContext,
    TimeProvider timeProvider)
    : IRequestHandler<RevokeHealthRecordAccessCommand, HealthRecordAccessDto>
{
    public async Task<HealthRecordAccessDto> Handle(RevokeHealthRecordAccessCommand request, CancellationToken ct)
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

        var activeGrant = await healthRecordAccessRepository.GetLatestGrantAsync(
            healthRecord.Id,
            doctor.Id,
            ct);

        if (activeGrant is null || !activeGrant.IsActive)
        {
            throw new NotFoundException(
                HealthRecordErrorCodes.HealthRecordAccessNotFound,
                "Active health record access grant was not found.");
        }

        var revokedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        activeGrant.Revoke(revokedAtUtc);
        await healthRecordAccessRepository.UpdateAsync(activeGrant, ct);

        await auditLogRepository.AppendAsync(
            AuditLog.Create(
                patient.Id,
                AuditActorType.Patient,
                AuditActions.HealthRecordAccessRevoked,
                "health_record",
                healthRecord.Id,
                revokedAtUtc,
                auditContext.IpAddress,
                auditContext.UserAgent,
                JsonSerializer.Serialize(new { doctor_id = doctor.Id })),
            ct);

        return activeGrant.ToDto(doctor.FullName);
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
