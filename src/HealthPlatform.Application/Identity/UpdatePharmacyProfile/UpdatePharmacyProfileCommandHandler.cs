using MediatR;

namespace HealthPlatform.Application.Identity.UpdatePharmacyProfile;

public sealed class UpdatePharmacyProfileCommandHandler(IPharmacyProfileUpdateWorkflow workflow)
    : IRequestHandler<UpdatePharmacyProfileCommand, PharmacyProfileDto>
{
    public Task<PharmacyProfileDto> Handle(UpdatePharmacyProfileCommand request, CancellationToken ct) =>
        workflow.UpdateAsync(request, ct);
}
