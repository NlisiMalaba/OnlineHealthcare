using MediatR;

namespace HealthPlatform.Application.Auth;

public sealed class LoginCommandHandler(IAuthLoginWorkflow workflow) : IRequestHandler<LoginCommand, LoginResponseDto>
{
    public Task<LoginResponseDto> Handle(LoginCommand request, CancellationToken ct) =>
        workflow.LoginAsync(request, ct);
}
