using HealthPlatform.Application.Payments.Instalments;
using HealthPlatform.Application.Payments.Instalments.CreateInstalmentPlan;
using HealthPlatform.Application.Payments.Instalments.GetInstalmentPlan;
using HealthPlatform.Application.Payments.Instalments.ListPatientInstalmentPlans;
using HealthPlatform.Application.Payments.Instalments.PreviewInstalmentPlan;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Payments.Instalments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/payments/instalments")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class InstalmentPlansController(ISender sender) : ControllerBase
{
    [HttpGet("preview")]
    [ProducesResponseType(typeof(InstalmentPlanPreviewDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InstalmentPlanPreviewDto>> PreviewAsync(
        [FromQuery] PreviewInstalmentPlanRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(
            new PreviewInstalmentPlanQuery(
                request.TotalAmountMinorUnits,
                request.Frequency,
                request.InstalmentCount,
                request.Currency,
                request.FirstDueDate),
            ct));

    [HttpPost]
    [ProducesResponseType(typeof(InstalmentPlanDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InstalmentPlanDto>> CreateAsync(
        [FromBody] CreateInstalmentPlanRequest request,
        CancellationToken ct)
    {
        var plan = await sender.Send(
            new CreateInstalmentPlanCommand(
                request.TotalAmountMinorUnits,
                request.Frequency,
                request.InstalmentCount,
                request.Currency,
                request.FirstDueDate,
                request.AppointmentId,
                request.MedicationOrderId,
                request.LabOrderId),
            ct);

        return Created($"/api/v1/payments/instalments/{plan.Id}", plan);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InstalmentPlanListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InstalmentPlanListItemDto>>> ListAsync(CancellationToken ct) =>
        Ok(await sender.Send(new ListPatientInstalmentPlansQuery(), ct));

    [HttpGet("{planId:guid}")]
    [ProducesResponseType(typeof(InstalmentPlanDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InstalmentPlanDto>> GetAsync(Guid planId, CancellationToken ct) =>
        Ok(await sender.Send(new GetInstalmentPlanQuery(planId), ct));
}

public sealed record PreviewInstalmentPlanRequest(
    long TotalAmountMinorUnits,
    InstalmentFrequency Frequency,
    int InstalmentCount,
    string Currency,
    DateOnly FirstDueDate);

public sealed record CreateInstalmentPlanRequest(
    long TotalAmountMinorUnits,
    InstalmentFrequency Frequency,
    int InstalmentCount,
    string Currency,
    DateOnly FirstDueDate,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId);
