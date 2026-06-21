using HealthPlatform.Application.Identity.UpdatePharmacyProfile;

namespace HealthPlatform.Application.Identity;

public interface IPharmacyProfileUpdateWorkflow
{
    Task<PharmacyProfileDto> UpdateAsync(UpdatePharmacyProfileCommand command, CancellationToken ct);
}
