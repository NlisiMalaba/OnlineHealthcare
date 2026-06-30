using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Payments.Instalments.GetInstalmentPlan;

public sealed class GetInstalmentPlanQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IInstalmentPlanRepository planRepository,
    IInstalmentPaymentRepository paymentRepository)
    : IRequestHandler<GetInstalmentPlanQuery, InstalmentPlanDto>
{
    public async Task<InstalmentPlanDto> Handle(GetInstalmentPlanQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var plan = await planRepository.GetByIdForPatientAsync(request.PlanId, patient.Id, ct)
            ?? throw new NotFoundException(
                InstalmentErrorCodes.PlanNotFound,
                "Instalment plan was not found.");

        var payments = await paymentRepository.ListForPlanAsync(plan.Id, ct);
        return plan.ToDto(payments);
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
