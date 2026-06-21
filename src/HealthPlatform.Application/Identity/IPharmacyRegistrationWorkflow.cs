using HealthPlatform.Application.Identity.RegisterPharmacy;

namespace HealthPlatform.Application.Identity;

public interface IPharmacyRegistrationWorkflow
{
    Task<PharmacyRegistrationResponseDto> RegisterAsync(RegisterPharmacyCommand command, CancellationToken ct);
}
