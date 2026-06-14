using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Domain.Identity;

namespace HealthPlatform.Application.Identity;

public interface IPatientProfileUpdateWorkflow
{
    Task<PatientProfileDto> UpdateAsync(UpdatePatientProfileCommand command, CancellationToken ct);
}
