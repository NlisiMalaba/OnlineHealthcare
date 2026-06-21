using HealthPlatform.Application.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
[AllowAnonymous]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> LoginAsync(
        [FromBody] LoginCommand command,
        CancellationToken ct) =>
        Ok(await sender.Send(command, ct));

    [HttpPost("login/mfa")]
    public async Task<ActionResult<LoginResponseDto>> CompleteMfaAsync(
        [FromBody] CompleteMfaLoginCommand command,
        CancellationToken ct) =>
        Ok(await sender.Send(command, ct));

    [HttpPost("login/device")]
    public async Task<ActionResult<LoginResponseDto>> CompleteDeviceLoginAsync(
        [FromBody] CompleteDeviceLoginCommand command,
        CancellationToken ct) =>
        Ok(await sender.Send(command, ct));
}
