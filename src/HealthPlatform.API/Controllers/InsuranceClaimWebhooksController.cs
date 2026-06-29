using HealthPlatform.Application.Insurance.Webhooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/insurance/webhooks")]
[AllowAnonymous]
public sealed class InsuranceClaimWebhooksController(ISender sender) : ControllerBase
{
    [HttpPost("{insurerCode}")]
    [ProducesResponseType(typeof(ProcessInsuranceClaimWebhookResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ProcessInsuranceClaimWebhookResultDto>> ProcessAsync(
        string insurerCode,
        CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);
        var headers = Request.Headers.ToDictionary(
            header => header.Key,
            header => header.Value.ToString(),
            StringComparer.OrdinalIgnoreCase);

        var result = await sender.Send(
            new ProcessInsuranceClaimWebhookCommand(insurerCode, rawBody, headers),
            ct);

        return Ok(result);
    }
}
