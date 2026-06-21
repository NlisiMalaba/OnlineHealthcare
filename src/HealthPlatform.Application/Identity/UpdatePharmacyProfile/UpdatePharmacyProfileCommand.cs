using HealthPlatform.Application.Behaviors;

namespace HealthPlatform.Application.Identity.UpdatePharmacyProfile;

public sealed record PharmacyLogoUpload(Stream Content, string ContentType, string FileName, long Length)
    : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync();
    }
}

public sealed record UpdatePharmacyProfileCommand(
    string? Name,
    string? Address,
    double? Latitude,
    double? Longitude,
    string? PhoneNumber,
    PharmacyLogoUpload? Logo) : ICommand<PharmacyProfileDto>;
