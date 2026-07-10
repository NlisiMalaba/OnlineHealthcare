using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.UpdateBirthPlan;

public sealed class UpdateBirthPlanCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IBirthPlanRepository birthPlanRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<UpdateBirthPlanCommand, BirthPlanDto>
{
    public async Task<BirthPlanDto> Handle(UpdateBirthPlanCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var antenatalRecord = await antenatalRecordRepository.GetByIdAsync(request.AntenatalRecordId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.AntenatalRecordNotFound,
                "Antenatal record was not found.");

        if (antenatalRecord.PatientId != patient.Id)
        {
            throw new AccessDeniedException(
                BirthPlanErrorCodes.AccessDenied,
                "You can only update your own birth plan.");
        }

        var birthPlan = await birthPlanRepository.GetByAntenatalRecordIdAsync(antenatalRecord.Id, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.BirthPlanNotFound,
                "Birth plan was not found.");

        var updatedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        birthPlan.Update(request.Content.ToDomain(), antenatalRecord.ObstetricDoctorId, updatedAtUtc);
        await birthPlanRepository.UpdateAsync(birthPlan, ct);

        foreach (var domainEvent in birthPlan.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }
        birthPlan.ClearDomainEvents();

        return birthPlan.ToDto();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
