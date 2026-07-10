using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using HealthPlatform.Domain.Maternal;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.CreateBirthPlan;

public sealed class CreateBirthPlanCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IAntenatalRecordRepository antenatalRecordRepository,
    IBirthPlanRepository birthPlanRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CreateBirthPlanCommand, BirthPlanDto>
{
    public async Task<BirthPlanDto> Handle(CreateBirthPlanCommand request, CancellationToken ct)
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
                "You can only create a birth plan for your own antenatal record.");
        }

        if (antenatalRecord.Status != AntenatalRecordStatus.Active)
        {
            throw new DomainException(
                BirthPlanErrorCodes.AntenatalRecordNotActive,
                "Birth plans can only be created for active antenatal records.");
        }

        var existingBirthPlan = await birthPlanRepository.GetByAntenatalRecordIdAsync(antenatalRecord.Id, ct);
        if (existingBirthPlan is not null)
        {
            throw new ConflictException(
                BirthPlanErrorCodes.BirthPlanAlreadyExists,
                "A birth plan already exists for this antenatal record.");
        }

        var createdAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var birthPlan = BirthPlan.Create(
            patient.Id,
            antenatalRecord.Id,
            request.Content.ToDomain(),
            createdAtUtc);

        await birthPlanRepository.AddAsync(birthPlan, ct);
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
