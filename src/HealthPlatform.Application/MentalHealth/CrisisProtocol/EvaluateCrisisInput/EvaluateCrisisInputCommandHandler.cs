using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.CrisisProtocol.EvaluateCrisisInput;

public sealed class EvaluateCrisisInputCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    ICrisisProtocolService crisisProtocolService)
    : IRequestHandler<EvaluateCrisisInputCommand, CrisisProtocolDto>
{
    public async Task<CrisisProtocolDto> Handle(EvaluateCrisisInputCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        return await crisisProtocolService.TryTriggerAsync(
            patient.Id,
            request.InputText,
            CrisisProtocolInputSource.AiAssistant,
            ct);
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                CrisisProtocolErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
