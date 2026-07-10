using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.AntenatalRecords.CreateAntenatalRecord;

public sealed class CreateAntenatalRecordCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<CreateAntenatalRecordCommand, AntenatalRecordDto>
{
    public async Task<AntenatalRecordDto> Handle(CreateAntenatalRecordCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var obstetricDoctor = await ResolveObstetricDoctorAsync(request.ObstetricDoctorId, ct);

        var existingActiveRecord = await antenatalRecordRepository.GetActiveByPatientIdAsync(patient.Id, ct);
        if (existingActiveRecord is not null)
        {
            throw new ConflictException(
                AntenatalRecordErrorCodes.ActiveRecordExists,
                "An active antenatal record already exists for this patient.");
        }

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var record = AntenatalRecord.Create(
            patient.Id,
            request.EstimatedDueDate,
            request.GestationalAgeWeeks,
            obstetricDoctor.Id,
            createdAtUtc);

        await antenatalRecordRepository.AddAsync(record, ct);
        await GenerateCheckupScheduleAsync(record, createdAtUtc, ct);

        foreach (var domainEvent in record.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }
        record.ClearDomainEvents();

        var scheduleEntries = await antenatalRecordRepository.ListScheduleEntriesByRecordIdAsync(record.Id, ct);
        return record.ToDto(scheduleEntries);
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }

    private async Task<Doctor> ResolveObstetricDoctorAsync(Guid obstetricDoctorId, CancellationToken ct)
    {
        var doctor = await doctorRepository.GetByIdAsync(obstetricDoctorId, ct)
            ?? throw new NotFoundException(
                AntenatalRecordErrorCodes.DoctorNotFound,
                "Obstetric doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                AntenatalRecordErrorCodes.DoctorNotVerified,
                "Only verified doctors can be assigned as obstetric doctors.");
        }

        if (!ObstetricPolicies.IsLicensedObstetrician(doctor.Specialty))
        {
            throw new DomainException(
                AntenatalRecordErrorCodes.DoctorNotObstetrician,
                "Assigned doctor must be a licensed obstetrician.");
        }

        return doctor;
    }

    private async Task GenerateCheckupScheduleAsync(
        AntenatalRecord record,
        DateTime createdAtUtc,
        CancellationToken ct)
    {
        var asOfDate = DateOnly.FromDateTime(createdAtUtc);
        var scheduleItems = AntenatalCheckupSchedulePolicies.BuildRecommendedSchedule(
            record.GestationalAgeWeeks,
            record.EstimatedDueDate,
            asOfDate);

        if (scheduleItems.Count == 0)
        {
            return;
        }

        var entries = scheduleItems
            .Select(item => AntenatalCheckupScheduleEntry.Create(
                record.Id,
                item.GestationalAgeWeeks,
                item.RecommendedDate,
                item.Description,
                createdAtUtc))
            .ToList();

        await antenatalRecordRepository.AddScheduleEntriesAsync(entries, ct);
    }
}
