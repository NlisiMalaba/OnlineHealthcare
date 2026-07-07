using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Labs.AnnotateDiagnosticReport;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/labs/annotations")]
[Authorize(Policy = AuthorizationPolicies.Doctor)]
public sealed class LabAnnotationsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(HealthRecordEntryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<HealthRecordEntryDto>> CreateAsync(
        [FromBody] CreateLabAnnotationRequest request,
        CancellationToken ct)
    {
        var entry = await sender.Send(
            new AnnotateDiagnosticReportCommand(
                request.TargetType,
                request.TargetId,
                request.Note),
            ct);

        return Created($"/api/v1/health-records/entries/{entry.Id}", entry);
    }
}

public sealed record CreateLabAnnotationRequest(
    DiagnosticAnnotationTargetType TargetType,
    Guid TargetId,
    string Note);
