using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.UpdatePharmacyProfile;

namespace HealthPlatform.API.Mapping;

public static class UpdatePharmacyProfileCommandMapper
{
    public static UpdatePharmacyProfileCommand ToCommand(UpdatePharmacyProfileRequest request)
    {
        PharmacyLogoUpload? logo = null;
        if (request.Logo is { Length: > 0 })
        {
            logo = new PharmacyLogoUpload(
                request.Logo.OpenReadStream(),
                request.Logo.ContentType,
                request.Logo.FileName,
                request.Logo.Length);
        }

        return new UpdatePharmacyProfileCommand(
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.PhoneNumber,
            logo);
    }
}
