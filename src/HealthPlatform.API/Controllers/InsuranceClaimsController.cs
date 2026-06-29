using HealthPlatform.Application.Insurance;
using HealthPlatform.Application.Insurance.GetInsuranceClaim;
using HealthPlatform.Application.Insurance.ListPatientInsuranceClaims;
using HealthPlatform.Application.Insurance.SubmitInsuranceClaim;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Insurance;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/insurance/claims")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class InsuranceClaimsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(InsuranceClaimDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<InsuranceClaimDto>> SubmitAsync(
        [FromBody] SubmitInsuranceClaimRequest request,
        CancellationToken ct)
    {
        var claim = await sender.Send(
            new SubmitInsuranceClaimCommand(
                request.InsurerCode,
                request.ClaimType,
                request.AmountMinorUnits,
                request.Currency,
                request.AppointmentId,
                request.MedicationOrderId,
                request.LabOrderId),
            ct);

        return Created($"/api/v1/insurance/claims/{claim.Id}", claim);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<InsuranceClaimListItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<InsuranceClaimListItemDto>>> ListAsync(CancellationToken ct) =>
        Ok(await sender.Send(new ListPatientInsuranceClaimsQuery(), ct));

    [HttpGet("{claimId:guid}")]
    [ProducesResponseType(typeof(InsuranceClaimDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<InsuranceClaimDto>> GetAsync(Guid claimId, CancellationToken ct) =>
        Ok(await sender.Send(new GetInsuranceClaimQuery(claimId), ct));
}

public sealed record SubmitInsuranceClaimRequest(
    string InsurerCode,
    InsuranceClaimType ClaimType,
    long AmountMinorUnits,
    string Currency,
    Guid? AppointmentId,
    Guid? MedicationOrderId,
    Guid? LabOrderId);
