using HealthPlatform.API.Requests.NextOfKin;
using HealthPlatform.Application.NextOfKin.AddNextOfKinContact;
using HealthPlatform.Application.NextOfKin.UpdateNextOfKinContact;

namespace HealthPlatform.API.Mapping;

public static class NextOfKinCommandMapper
{
    public static AddNextOfKinContactCommand ToCreateCommand(NextOfKinContactUpsertRequest request) =>
        new(
            request.FullName,
            request.Relationship,
            request.PhoneNumber,
            request.Email,
            request.IsMentalHealthContact);

    public static UpdateNextOfKinContactCommand ToUpdateCommand(
        Guid contactId,
        NextOfKinContactUpsertRequest request) =>
        new(
            contactId,
            request.FullName,
            request.Relationship,
            request.PhoneNumber,
            request.Email,
            request.IsMentalHealthContact);
}
