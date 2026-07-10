using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.ConfigureFetalMonitoringReminders;

public sealed class ConfigureFetalMonitoringRemindersCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    TimeProvider timeProvider)
    : IRequestHandler<ConfigureFetalMonitoringRemindersCommand, AntenatalRecordDto>
{
    public async Task<AntenatalRecordDto> Handle(ConfigureFetalMonitoringRemindersCommand request, CancellationToken ct)
    {
        var doctor = await ResolveObstetricDoctorAsync(ct);
        var record = await antenatalRecordRepository.GetByIdAsync(request.AntenatalRecordId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.AntenatalRecordNotFound,
                "Antenatal record was not found.");

        if (record.ObstetricDoctorId != doctor.Id)
        {
            throw new AccessDeniedException(
                AntenatalRecordErrorCodes.ObstetricDoctorAccessDenied,
                "Only the assigned obstetric doctor can configure fetal monitoring reminders.");
        }

        if (record.Status != AntenatalRecordStatus.Active)
        {
            throw new DomainException(
                AntenatalRecordErrorCodes.AntenatalRecordNotActive,
                "Fetal monitoring reminders can only be configured for active antenatal records.");
        }

        var configuredAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        record.ConfigureFetalMonitoringReminders(request.IntervalDays, configuredAtUtc);
        await antenatalRecordRepository.UpdateAsync(record, ct);

        var scheduleEntries = await antenatalRecordRepository.ListScheduleEntriesByRecordIdAsync(record.Id, ct);
        return record.ToDto(scheduleEntries);
    }

    private async Task<Doctor> ResolveObstetricDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                AntenatalRecordErrorCodes.DoctorNotVerified,
                "Only verified doctors can configure fetal monitoring reminders.");
        }

        if (!ObstetricPolicies.IsLicensedObstetrician(doctor.Specialty))
        {
            throw new DomainException(
                AntenatalRecordErrorCodes.DoctorNotObstetrician,
                "Only licensed obstetricians can configure fetal monitoring reminders.");
        }

        return doctor;
    }
}
