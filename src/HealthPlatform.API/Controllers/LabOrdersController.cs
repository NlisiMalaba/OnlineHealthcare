using HealthPlatform.Application.Labs;
using HealthPlatform.Application.Labs.ApprovePatientLabOrder;
using HealthPlatform.Application.Labs.CreateDoctorLabOrder;
using HealthPlatform.Application.Labs.CreatePatientLabOrderRequest;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/labs/orders")]
public sealed class LabOrdersController(ISender sender) : ControllerBase
{
    [HttpPost("doctor")]
    [Authorize(Policy = AuthorizationPolicies.Doctor)]
    [ProducesResponseType(typeof(LabOrderDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<LabOrderDto>> CreateDoctorOrderedAsync(
        [FromBody] CreateDoctorLabOrderRequest request,
        CancellationToken ct)
    {
        var order = await sender.Send(
            new CreateDoctorLabOrderCommand(
                request.PatientId,
                request.HealthRecordId,
                request.LabPartnerCode,
                request.TestCode,
                request.ClinicalNotes),
            ct);

        return Created($"/api/v1/labs/orders/{order.Id}", order);
    }

    [HttpPost("patient-request")]
    [Authorize(Policy = AuthorizationPolicies.Patient)]
    [ProducesResponseType(typeof(LabOrderDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<LabOrderDto>> CreatePatientRequestAsync(
        [FromBody] CreatePatientLabOrderRequest request,
        CancellationToken ct)
    {
        var order = await sender.Send(
            new CreatePatientLabOrderRequestCommand(
                request.LabPartnerCode,
                request.TestCode,
                request.ClinicalNotes),
            ct);

        return Created($"/api/v1/labs/orders/{order.Id}", order);
    }

    [HttpPost("{labOrderId:guid}/approve")]
    [Authorize(Policy = AuthorizationPolicies.Doctor)]
    [ProducesResponseType(typeof(LabOrderDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<LabOrderDto>> ApprovePatientRequestAsync(Guid labOrderId, CancellationToken ct) =>
        Ok(await sender.Send(new ApprovePatientLabOrderCommand(labOrderId), ct));
}

public sealed record CreateDoctorLabOrderRequest(
    Guid PatientId,
    Guid HealthRecordId,
    string LabPartnerCode,
    string TestCode,
    string? ClinicalNotes);

public sealed record CreatePatientLabOrderRequest(
    string LabPartnerCode,
    string TestCode,
    string? ClinicalNotes);
