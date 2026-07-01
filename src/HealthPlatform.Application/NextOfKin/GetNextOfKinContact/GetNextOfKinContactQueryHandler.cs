using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.NextOfKin.GetNextOfKinContact;

public sealed class GetNextOfKinContactQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    INextOfKinRepository nextOfKinRepository)
    : IRequestHandler<GetNextOfKinContactQuery, NextOfKinContactDto>
{
    public async Task<NextOfKinContactDto> Handle(GetNextOfKinContactQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var contact = await nextOfKinRepository.GetByIdForPatientAsync(request.ContactId, patient.Id, ct)
            ?? throw new NotFoundException(
                NextOfKinErrorCodes.ContactNotFound,
                "Next-of-kin contact was not found.");

        return contact.ToDto();
    }

    private async Task<Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                NextOfKinErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
