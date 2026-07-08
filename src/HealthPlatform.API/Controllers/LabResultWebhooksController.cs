using HealthPlatform.Application.Labs.Webhooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/labs/webhooks/results")]
[AllowAnonymous]
public sealed class LabResultWebhooksController(ISender sender) : ControllerBase
{
    [HttpPost("{labPartnerCode}")]
    [ProducesResponseType(typeof(IngestLabResultWebhookResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IngestLabResultWebhookResultDto>> IngestAsync(
        string labPartnerCode,
        [FromForm] IngestLabResultWebhookRequest request,
        CancellationToken ct)
    {
        await using var stream = request.ResultFile.OpenReadStream();
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);

        var result = await sender.Send(
            new IngestLabResultWebhookCommand(
                labPartnerCode,
                request.LabPartnerOrderReference,
                request.TestCode,
                ms.ToArray(),
                request.ResultFile.ContentType,
                request.ResultFile.FileName,
                request.IsCritical),
            ct);

        return Ok(result);
    }
}

public sealed record IngestLabResultWebhookRequest(
    string LabPartnerOrderReference,
    string TestCode,
    bool IsCritical,
    IFormFile ResultFile);
