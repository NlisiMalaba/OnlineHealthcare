using HealthPlatform.Application.Identity.RegisterPatient;
using MediatR;

namespace HealthPlatform.Application.Identity.RegisterPatient;

public sealed class RegisterPatientCommandHandler(IPatientRegistrationWorkflow workflow)
    : IRequestHandler<RegisterPatientCommand, PatientRegistrationResponseDto>
{
    public Task<PatientRegistrationResponseDto> Handle(RegisterPatientCommand request, CancellationToken ct) =>
        workflow.RegisterAsync(request, ct);
}
