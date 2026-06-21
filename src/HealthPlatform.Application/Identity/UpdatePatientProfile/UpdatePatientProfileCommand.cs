using HealthPlatform.Application.Behaviors;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity.UpdatePatientProfile;

public sealed record ProfilePhotoUpload(Stream Content, string ContentType, string FileName, long Length)
    : IAsyncDisposable
{
    public async ValueTask DisposeAsync()
    {
        await Content.DisposeAsync();
    }
}

public sealed record UpdatePatientProfileCommand(
    string? FullName,
    DateOnly? DateOfBirth,
    BloodType? BloodType,
    IReadOnlyList<string>? KnownAllergies,
    IReadOnlyList<string>? ChronicConditions,
    ProfilePhotoUpload? ProfilePhoto) : ICommand<PatientProfileDto>;
