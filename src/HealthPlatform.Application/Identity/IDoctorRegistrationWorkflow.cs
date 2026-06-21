using HealthPlatform.Application.Identity.RegisterDoctor;

namespace HealthPlatform.Application.Identity;

public interface IDoctorRegistrationWorkflow
{
    Task<DoctorRegistrationResponseDto> RegisterAsync(RegisterDoctorCommand command, CancellationToken ct);
}
