using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.Payments.CreditLine.GetCreditLine;

public sealed class GetCreditLineQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IPatientCreditLineRepository creditLineRepository)
    : IRequestHandler<GetCreditLineQuery, CreditLineDto>
{
    public async Task<CreditLineDto> Handle(GetCreditLineQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var creditLine = await creditLineRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(
                CreditLineErrorCodes.CreditLineNotFound,
                "Patient does not have an active credit line.");

        return creditLine.ToDto();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
