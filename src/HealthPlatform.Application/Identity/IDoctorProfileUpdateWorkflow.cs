using HealthPlatform.Application.Identity.UpdateDoctorProfile;

namespace HealthPlatform.Application.Identity;

public interface IDoctorProfileUpdateWorkflow
{
    Task<DoctorProfileDto> UpdateAsync(UpdateDoctorProfileCommand command, CancellationToken ct);
}
