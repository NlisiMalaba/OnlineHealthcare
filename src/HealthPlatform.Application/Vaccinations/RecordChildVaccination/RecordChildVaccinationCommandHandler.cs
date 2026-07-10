using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.ChildProfiles;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Domain.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.RecordChildVaccination;

public sealed class RecordChildVaccinationCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IChildProfileRepository childProfileRepository,
    IVaccinationScheduleRepository scheduleRepository,
    IVaccinationRecordRepository recordRepository,
    TimeProvider timeProvider)
    : IRequestHandler<RecordChildVaccinationCommand, VaccinationRecordDto>
{
    public async Task<VaccinationRecordDto> Handle(RecordChildVaccinationCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var childProfile = await childProfileRepository.GetByIdAsync(request.ChildProfileId, ct)
            ?? throw new NotFoundException(
                VaccinationErrorCodes.ChildProfileNotFound,
                "Child profile was not found.");

        await EnsureCanRecordAsync(userId, childProfile, ct);

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var scheduleEntry = await ResolveScheduleEntryAsync(request, childProfile.Id, ct);

        var record = VaccinationRecord.CreateForChild(
            childProfile.Id,
            scheduleEntry?.Id,
            request.VaccineName,
            request.AdministeredDate,
            request.BatchNumber,
            request.Provider,
            userId,
            createdAtUtc);

        await recordRepository.AddAsync(record, ct);

        if (scheduleEntry is not null)
        {
            scheduleEntry.MarkCompleted(record.Id, createdAtUtc);
            await scheduleRepository.UpdateAsync(scheduleEntry, ct);
        }

        await recordRepository.SaveChangesAsync(ct);
        return record.ToDto();
    }

    private async Task EnsureCanRecordAsync(
        Guid userId,
        Domain.Maternal.ChildProfile childProfile,
        CancellationToken ct)
    {
        var guardian = await patientRepository.GetByUserIdAsync(userId, ct);
        if (guardian is not null && guardian.Id == childProfile.GuardianId)
        {
            return;
        }

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct);
        if (doctor is not null)
        {
            return;
        }

        throw new AccessDeniedException(
            VaccinationErrorCodes.AccessDenied,
            "Only the guardian or a doctor can record child vaccinations.");
    }

    private async Task<VaccinationScheduleEntry?> ResolveScheduleEntryAsync(
        RecordChildVaccinationCommand request,
        Guid childProfileId,
        CancellationToken ct)
    {
        if (!request.ScheduleEntryId.HasValue)
        {
            return null;
        }

        var scheduleEntry = await scheduleRepository.GetByIdAsync(request.ScheduleEntryId.Value, ct)
            ?? throw new NotFoundException(
                VaccinationErrorCodes.ScheduleEntryNotFound,
                "Vaccination schedule entry was not found.");

        if (scheduleEntry.CompletedAtUtc.HasValue)
        {
            throw new ConflictException(
                VaccinationErrorCodes.ScheduleEntryCompleted,
                "Vaccination schedule entry is already completed.");
        }

        if (scheduleEntry.ChildProfileId != childProfileId
            || !string.Equals(scheduleEntry.VaccineName, request.VaccineName, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                VaccinationErrorCodes.ScheduleEntryMismatch,
                "Vaccination schedule entry does not match the child profile or vaccine name.");
        }

        return scheduleEntry;
    }
}
