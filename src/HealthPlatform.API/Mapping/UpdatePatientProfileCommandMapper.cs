using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.UpdatePatientProfile;

namespace HealthPlatform.API.Mapping;

public static class UpdatePatientProfileCommandMapper
{
    public static UpdatePatientProfileCommand ToCommand(UpdatePatientProfileRequest request)
    {
        ProfilePhotoUpload? photo = null;
        if (request.Photo is { Length: > 0 })
        {
            photo = new ProfilePhotoUpload(
                request.Photo.OpenReadStream(),
                request.Photo.ContentType,
                request.Photo.FileName,
                request.Photo.Length);
        }

        return new UpdatePatientProfileCommand(
            request.FullName,
            request.DateOfBirth,
            request.BloodType,
            request.KnownAllergies,
            request.ChronicConditions,
            photo);
    }
}
