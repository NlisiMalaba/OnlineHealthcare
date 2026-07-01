using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.NextOfKin.UpdateNextOfKinContact;

public sealed class UpdateNextOfKinContactCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    INextOfKinRepository nextOfKinRepository)
    : IRequestHandler<UpdateNextOfKinContactCommand, NextOfKinContactDto>
{
    public async Task<NextOfKinContactDto> Handle(UpdateNextOfKinContactCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var contact = await nextOfKinRepository.GetByIdForPatientAsync(request.ContactId, patient.Id, ct)
            ?? throw new NotFoundException(
                NextOfKinErrorCodes.ContactNotFound,
                "Next-of-kin contact was not found.");

        contact.Update(
            request.FullName,
            request.Relationship,
            request.PhoneNumber,
            request.Email,
            request.IsMentalHealthContact);

        await nextOfKinRepository.UpdateAsync(contact, ct);
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
