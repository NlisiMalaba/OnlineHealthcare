namespace HealthPlatform.Application.Auth;

public interface IAuthLoginWorkflow
{
    Task<LoginResponseDto> LoginAsync(LoginCommand command, CancellationToken ct);

    Task<LoginResponseDto> CompleteMfaAsync(CompleteMfaLoginCommand command, CancellationToken ct);

    Task<LoginResponseDto> CompleteDeviceLoginAsync(CompleteDeviceLoginCommand command, CancellationToken ct);
}
