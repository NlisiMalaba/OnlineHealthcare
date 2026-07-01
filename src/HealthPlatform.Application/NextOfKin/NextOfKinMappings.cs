using HealthPlatform.Domain.NextOfKin;

namespace HealthPlatform.Application.NextOfKin;

public static class NextOfKinMappings
{
    public static NextOfKinContactDto ToDto(this NextOfKinContact contact) =>
        new(
            contact.Id,
            contact.PatientId,
            contact.FullName,
            contact.Relationship,
            contact.PhoneNumber,
            contact.Email,
            contact.IsMentalHealthContact);
}
