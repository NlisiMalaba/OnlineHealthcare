using HealthPlatform.API.Requests.Telemedicine;
using HealthPlatform.Application.Telemedicine;
using HealthPlatform.Application.Telemedicine.JoinSession;
using HealthPlatform.Application.Telemedicine.EndSession;
using HealthPlatform.Application.Telemedicine.RecordingConsent;
using HealthPlatform.Application.Telemedicine.Realtime;
using HealthPlatform.Application.Telemedicine.Realtime.Files;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/appointments")]
[Authorize]
public sealed class TelemedicineSessionsController(ISender sender) : ControllerBase
{
    [HttpPost("{appointmentId:guid}/join")]
    [ProducesResponseType(typeof(JoinTelemedicineSessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<JoinTelemedicineSessionDto>> JoinAsync(
        Guid appointmentId,
        [FromBody] JoinTelemedicineSessionRequest? request,
        CancellationToken ct) =>
        Ok(await sender.Send(
            new JoinTelemedicineSessionCommand(appointmentId, request?.Mode),
            ct));

    [HttpPost("{appointmentId:guid}/recording-consent")]
    [ProducesResponseType(typeof(GrantRecordingConsentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GrantRecordingConsentDto>> GrantRecordingConsentAsync(
        Guid appointmentId,
        CancellationToken ct) =>
        Ok(await sender.Send(new GrantRecordingConsentCommand(appointmentId), ct));

    [HttpPost("{appointmentId:guid}/recording/enable")]
    [ProducesResponseType(typeof(EnableSessionRecordingDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnableSessionRecordingDto>> EnableRecordingAsync(
        Guid appointmentId,
        CancellationToken ct) =>
        Ok(await sender.Send(new EnableSessionRecordingCommand(appointmentId), ct));

    [HttpPost("{appointmentId:guid}/telemedicine/files")]
    [ProducesResponseType(typeof(TelemedicineFileSharedDto), StatusCodes.Status200OK)]
    [RequestSizeLimit(TelemedicinePolicies.MaxSharedFileBytes)]
    public async Task<ActionResult<TelemedicineFileSharedDto>> ShareFileAsync(
        Guid appointmentId,
        IFormFile file,
        CancellationToken ct)
    {
        await using var stream = file.OpenReadStream();
        return Ok(await sender.Send(
            new ShareTelemedicineSessionFileCommand(
                appointmentId,
                stream,
                file.ContentType,
                file.FileName,
                file.Length),
            ct));
    }

    [HttpPost("{appointmentId:guid}/telemedicine/end")]
    [ProducesResponseType(typeof(EndTelemedicineSessionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EndTelemedicineSessionDto>> EndAsync(
        Guid appointmentId,
        CancellationToken ct) =>
        Ok(await sender.Send(new EndTelemedicineSessionCommand(appointmentId), ct));
}
