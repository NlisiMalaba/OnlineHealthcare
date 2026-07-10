using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.RecordAntenatalCheckup;

public sealed class RecordAntenatalCheckupCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IAntenatalCheckupEntryRepository antenatalCheckupEntryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<RecordAntenatalCheckupCommand, AntenatalCheckupEntryDto>
{
    public async Task<AntenatalCheckupEntryDto> Handle(RecordAntenatalCheckupCommand request, CancellationToken ct)
    {
        var doctor = await ResolveObstetricDoctorAsync(ct);
        var record = await antenatalRecordRepository.GetByIdAsync(request.AntenatalRecordId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.AntenatalRecordNotFound,
                "Antenatal record was not found.");

        EnsureAssignedObstetricDoctor(record, doctor);

        if (record.Status != AntenatalRecordStatus.Active)
        {
            throw new DomainException(
                AntenatalRecordErrorCodes.AntenatalRecordNotActive,
                "Checkups can only be recorded for active antenatal records.");
        }

        var recordedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        AntenatalCheckupScheduleEntry? scheduleEntry = null;
        if (request.ScheduleEntryId.HasValue)
        {
            scheduleEntry = await antenatalRecordRepository.GetScheduleEntryByIdAsync(
                request.ScheduleEntryId.Value,
                ct)
                ?? throw new NotFoundException(
                    AntenatalRecordErrorCodes.ScheduleEntryNotFound,
                    "Antenatal checkup schedule entry was not found.");

            if (scheduleEntry.AntenatalRecordId != record.Id)
            {
                throw new DomainException(
                    AntenatalRecordErrorCodes.ScheduleEntryMismatch,
                    "Schedule entry does not belong to this antenatal record.");
            }

            if (scheduleEntry.CompletedAtUtc.HasValue)
            {
                throw new ConflictException(
                    AntenatalRecordErrorCodes.CheckupAlreadyCompleted,
                    "This scheduled checkup has already been completed.");
            }
        }

        var clinicalNotes = string.IsNullOrWhiteSpace(request.ClinicalNotes)
            ? null
            : request.ClinicalNotes.Trim();

        var entryReference = await antenatalCheckupEntryRepository.SaveAsync(
            new AntenatalCheckupEntryRecord(
                record.Id,
                request.ScheduleEntryId,
                record.PatientId,
                doctor.Id,
                request.GestationalAgeWeeks,
                request.FetalHeartRateBpm,
                request.FundalHeightCm,
                request.EstimatedFetalWeightGrams,
                request.BloodPressureSystolic,
                request.BloodPressureDiastolic,
                request.MaternalWeightKg,
                clinicalNotes,
                recordedAtUtc),
            ct);

        record.AddCheckupEntry(entryReference.DocumentId, recordedAtUtc);

        if (request.FetalMonitoringReminderIntervalDays.HasValue)
        {
            record.ConfigureFetalMonitoringReminders(
                request.FetalMonitoringReminderIntervalDays.Value,
                recordedAtUtc);
        }

        await antenatalRecordRepository.UpdateAsync(record, ct);

        if (scheduleEntry is not null)
        {
            try
            {
                scheduleEntry.MarkCompleted(entryReference.DocumentId, recordedAtUtc);
            }
            catch (AntenatalCheckupCompletionNotAllowedException ex)
            {
                throw new ConflictException(AntenatalRecordErrorCodes.CheckupAlreadyCompleted, ex.Message);
            }

            await antenatalRecordRepository.UpdateScheduleEntryAsync(scheduleEntry, ct);
        }

        return await antenatalCheckupEntryRepository.GetByIdAsync(entryReference.DocumentId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.AntenatalRecordNotFound,
                "Recorded antenatal checkup entry was not found.");
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
                "Only verified doctors can record antenatal checkups.");
        }

        if (!ObstetricPolicies.IsLicensedObstetrician(doctor.Specialty))
        {
            throw new DomainException(
                AntenatalRecordErrorCodes.DoctorNotObstetrician,
                "Only licensed obstetricians can record antenatal checkups.");
        }

        return doctor;
    }

    private static void EnsureAssignedObstetricDoctor(AntenatalRecord record, Doctor doctor)
    {
        if (record.ObstetricDoctorId != doctor.Id)
        {
            throw new AccessDeniedException(
                AntenatalRecordErrorCodes.ObstetricDoctorAccessDenied,
                "Only the assigned obstetric doctor can record checkups for this antenatal record.");
        }
    }
}
