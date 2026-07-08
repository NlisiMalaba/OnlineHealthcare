using HealthPlatform.Application.Labs.Webhooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/labs/webhooks/radiology")]
[AllowAnonymous]
public sealed class RadiologyReportWebhooksController(ISender sender) : ControllerBase
{
    [HttpPost("{labPartnerCode}")]
    [ProducesResponseType(typeof(IngestRadiologyReportWebhookResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IngestRadiologyReportWebhookResultDto>> IngestAsync(
        string labPartnerCode,
        [FromForm] IngestRadiologyReportWebhookRequest request,
        CancellationToken ct)
    {
        await using var reportStream = request.ReportFile.OpenReadStream();
        using var reportMs = new MemoryStream();
        await reportStream.CopyToAsync(reportMs, ct);

        var imagingPayloads = new List<RadiologyImagingUploadPayload>(request.ImagingFiles.Count);
        foreach (var file in request.ImagingFiles)
        {
            await using var fileStream = file.OpenReadStream();
            using var fileMs = new MemoryStream();
            await fileStream.CopyToAsync(fileMs, ct);
            imagingPayloads.Add(new RadiologyImagingUploadPayload(fileMs.ToArray(), file.ContentType, file.FileName));
        }

        var result = await sender.Send(
            new IngestRadiologyReportWebhookCommand(
                labPartnerCode,
                request.LabPartnerOrderReference,
                reportMs.ToArray(),
                request.ReportFile.ContentType,
                request.ReportFile.FileName,
                imagingPayloads),
            ct);

        return Ok(result);
    }
}

public sealed record IngestRadiologyReportWebhookRequest(
    string LabPartnerOrderReference,
    IFormFile ReportFile,
    IReadOnlyList<IFormFile> ImagingFiles);
