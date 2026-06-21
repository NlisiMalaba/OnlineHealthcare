using MediatR;

namespace HealthPlatform.Application.Identity.RegisterPharmacy;

public sealed class RegisterPharmacyCommandHandler(IPharmacyRegistrationWorkflow workflow)
    : IRequestHandler<RegisterPharmacyCommand, PharmacyRegistrationResponseDto>
{
    public Task<PharmacyRegistrationResponseDto> Handle(RegisterPharmacyCommand request, CancellationToken ct) =>
        workflow.RegisterAsync(request, ct);
}
