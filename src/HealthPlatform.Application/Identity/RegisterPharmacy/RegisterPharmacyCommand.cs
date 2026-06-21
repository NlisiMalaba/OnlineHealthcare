using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Identity.RegisterPharmacy;

public sealed record PharmacyFileUpload(Stream Content, string ContentType, string FileName, long Length)
    : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync();
    }
}

public sealed record RegisterPharmacyCommand(
    string Name,
    string Address,
    double? Latitude,
    double? Longitude,
    string Email,
    string PhoneNumber,
    string Password,
    PharmacyFileUpload? Logo) : ICommand<PharmacyRegistrationResponseDto>;
