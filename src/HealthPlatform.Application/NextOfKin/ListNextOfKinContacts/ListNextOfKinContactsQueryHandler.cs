using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.Identity;
using MediatR;

namespace HealthPlatform.Application.NextOfKin.ListNextOfKinContacts;

public sealed class ListNextOfKinContactsQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    INextOfKinRepository nextOfKinRepository)
    : IRequestHandler<ListNextOfKinContactsQuery, IReadOnlyList<NextOfKinContactDto>>
{
    public async Task<IReadOnlyList<NextOfKinContactDto>> Handle(ListNextOfKinContactsQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var contacts = await nextOfKinRepository.ListByPatientIdAsync(patient.Id, ct);
        return contacts.Select(contact => contact.ToDto()).ToList();
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
