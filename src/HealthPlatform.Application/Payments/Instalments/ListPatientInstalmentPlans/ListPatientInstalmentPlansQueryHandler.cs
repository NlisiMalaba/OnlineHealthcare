using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Payments.Instalments.ListPatientInstalmentPlans;

public sealed class ListPatientInstalmentPlansQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IInstalmentPlanRepository planRepository)
    : IRequestHandler<ListPatientInstalmentPlansQuery, IReadOnlyList<InstalmentPlanListItemDto>>
{
    public async Task<IReadOnlyList<InstalmentPlanListItemDto>> Handle(
        ListPatientInstalmentPlansQuery request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var plans = await planRepository.ListForPatientAsync(patient.Id, ct);
        return plans.Select(plan => plan.ToListItemDto()).ToList();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
