using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.NextOfKin;
using MediatR;

namespace HealthPlatform.Application.NextOfKin.AddNextOfKinContact;

public sealed class AddNextOfKinContactCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    INextOfKinRepository nextOfKinRepository,
    INextOfKinDesignationNotifier designationNotifier)
    : IRequestHandler<AddNextOfKinContactCommand, NextOfKinContactDto>
{
    public async Task<NextOfKinContactDto> Handle(AddNextOfKinContactCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        await EnsureContactLimitNotReachedAsync(patient.Id, ct);

        var contact = NextOfKinContact.Create(
            patient.Id,
            request.FullName,
            request.Relationship,
            request.PhoneNumber,
            request.Email,
            request.IsMentalHealthContact);

        await nextOfKinRepository.AddAsync(contact, ct);

        var contactDto = contact.ToDto();
        await designationNotifier.NotifyDesignatedAsync(contactDto, patient.FullName, ct);
        return contactDto;
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

    private async Task EnsureContactLimitNotReachedAsync(Guid patientId, CancellationToken ct)
    {
        var contactCount = await nextOfKinRepository.CountByPatientIdAsync(patientId, ct);
        if (contactCount >= NextOfKinPolicies.MaxContactsPerPatient)
        {
            throw new ConflictException(
                NextOfKinErrorCodes.MaxContactsReached,
                $"A patient may designate at most {NextOfKinPolicies.MaxContactsPerPatient} next-of-kin contacts.");
        }
    }
}
