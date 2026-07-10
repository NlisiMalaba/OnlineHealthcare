using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Vaccinations;
using HealthPlatform.Domain.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.RecordPatientVaccination;

public sealed class RecordPatientVaccinationCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IVaccinationScheduleRepository scheduleRepository,
    IVaccinationRecordRepository recordRepository,
    TimeProvider timeProvider)
    : IRequestHandler<RecordPatientVaccinationCommand, VaccinationRecordDto>
{
    public async Task<VaccinationRecordDto> Handle(RecordPatientVaccinationCommand request, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var patient = await ResolveTargetPatientAsync(userId, request.PatientId, ct);
        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var scheduleEntry = await ResolveScheduleEntryAsync(request, patient.Id, ct);

        var record = VaccinationRecord.CreateForPatient(
            patient.Id,
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

    private async Task<Domain.Identity.Patient> ResolveTargetPatientAsync(
        Guid userId,
        Guid? patientId,
        CancellationToken ct)
    {
        if (patientId.HasValue)
        {
            var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
                ?? throw new AccessDeniedException(
                    VaccinationErrorCodes.AccessDenied,
                    "Only doctors can record vaccinations for another patient.");

            return await patientRepository.GetByIdAsync(patientId.Value, ct)
                ?? throw new NotFoundException(
                    VaccinationErrorCodes.PatientNotFound,
                    "Patient profile was not found.");
        }

        var patient = await patientRepository.GetByUserIdAsync(userId, ct);
        if (patient is not null)
        {
            return patient;
        }

        var recordingDoctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct);
        if (recordingDoctor is not null)
        {
            throw new AccessDeniedException(
                VaccinationErrorCodes.AccessDenied,
                "Doctors must specify the patient id when recording vaccinations.");
        }

        throw new AccessDeniedException(
            VaccinationErrorCodes.AccessDenied,
            "Authenticated user cannot record patient vaccinations.");
    }

    private async Task<VaccinationScheduleEntry?> ResolveScheduleEntryAsync(
        RecordPatientVaccinationCommand request,
        Guid patientId,
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

        if (scheduleEntry.PatientId != patientId
            || !string.Equals(scheduleEntry.VaccineName, request.VaccineName, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                VaccinationErrorCodes.ScheduleEntryMismatch,
                "Vaccination schedule entry does not match the patient or vaccine name.");
        }

        return scheduleEntry;
    }
}
