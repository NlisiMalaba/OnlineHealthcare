using HealthPlatform.Application.Identity.RegisterDoctor;
using MediatR;

namespace HealthPlatform.Application.Identity.RegisterDoctor;

public sealed class RegisterDoctorCommandHandler(IDoctorRegistrationWorkflow workflow)
    : IRequestHandler<RegisterDoctorCommand, DoctorRegistrationResponseDto>
{
    public Task<DoctorRegistrationResponseDto> Handle(RegisterDoctorCommand request, CancellationToken ct) =>
        workflow.RegisterAsync(request, ct);
}
