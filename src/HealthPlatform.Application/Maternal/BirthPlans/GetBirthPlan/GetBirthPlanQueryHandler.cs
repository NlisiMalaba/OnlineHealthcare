using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Maternal.AntenatalRecords;
using MediatR;

namespace HealthPlatform.Application.Maternal.BirthPlans.GetBirthPlan;

public sealed class GetBirthPlanQueryHandler(
    IAntenatalRecordRepository antenatalRecordRepository,
    IBirthPlanRepository birthPlanRepository,
    IMaternalCareAccessGuard maternalCareAccessGuard)
    : IRequestHandler<GetBirthPlanQuery, BirthPlanDto>
{
    public async Task<BirthPlanDto> Handle(GetBirthPlanQuery request, CancellationToken ct)
    {
        var antenatalRecord = await antenatalRecordRepository.GetByIdAsync(request.AntenatalRecordId, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.AntenatalRecordNotFound,
                "Antenatal record was not found.");

        await maternalCareAccessGuard.EnsureCanReadBirthPlanAsync(
            antenatalRecord.Id,
            antenatalRecord.PatientId,
            antenatalRecord.ObstetricDoctorId,
            ct);

        var birthPlan = await birthPlanRepository.GetByAntenatalRecordIdAsync(antenatalRecord.Id, ct)
            ?? throw new NotFoundException(
                BirthPlanErrorCodes.BirthPlanNotFound,
                "Birth plan was not found.");

        return birthPlan.ToDto();
    }
}
