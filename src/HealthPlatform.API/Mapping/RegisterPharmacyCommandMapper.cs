using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterPharmacy;

namespace HealthPlatform.API.Mapping;

public static class RegisterPharmacyCommandMapper
{
    public static RegisterPharmacyCommand ToCommand(RegisterPharmacyRequest request)
    {
        PharmacyFileUpload? logo = null;
        if (request.Logo is { Length: > 0 })
        {
            logo = new PharmacyFileUpload(
                request.Logo.OpenReadStream(),
                request.Logo.ContentType,
                request.Logo.FileName,
                request.Logo.Length);
        }

        return new RegisterPharmacyCommand(
            request.Name,
            request.Address,
            request.Latitude,
            request.Longitude,
            request.Email,
            request.PhoneNumber,
            request.Password,
            logo);
    }
}
