using HealthPlatform.Application.Identity.RegisterPatient;

namespace HealthPlatform.Application.Identity;

public interface IPatientRegistrationWorkflow
{
    Task<PatientRegistrationResponseDto> RegisterAsync(RegisterPatientCommand command, CancellationToken ct);
}
