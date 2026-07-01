using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.NextOfKin;
using HealthPlatform.Application.NextOfKin;
using HealthPlatform.Application.NextOfKin.DeleteNextOfKinContact;
using HealthPlatform.Application.NextOfKin.GetNextOfKinContact;
using HealthPlatform.Application.NextOfKin.ListNextOfKinContacts;
using HealthPlatform.Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/next-of-kin")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class NextOfKinController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NextOfKinContactDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<NextOfKinContactDto>>> ListAsync(CancellationToken ct) =>
        Ok(await sender.Send(new ListNextOfKinContactsQuery(), ct));

    [HttpGet("{contactId:guid}")]
    [ProducesResponseType(typeof(NextOfKinContactDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NextOfKinContactDto>> GetAsync(Guid contactId, CancellationToken ct) =>
        Ok(await sender.Send(new GetNextOfKinContactQuery(contactId), ct));

    [HttpPost]
    [ProducesResponseType(typeof(NextOfKinContactDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<NextOfKinContactDto>> CreateAsync(
        [FromBody] NextOfKinContactUpsertRequest request,
        CancellationToken ct)
    {
        var contact = await sender.Send(NextOfKinCommandMapper.ToCreateCommand(request), ct);
        return CreatedAtAction(nameof(GetAsync), new { contactId = contact.Id }, contact);
    }

    [HttpPut("{contactId:guid}")]
    [ProducesResponseType(typeof(NextOfKinContactDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NextOfKinContactDto>> UpdateAsync(
        Guid contactId,
        [FromBody] NextOfKinContactUpsertRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(NextOfKinCommandMapper.ToUpdateCommand(contactId, request), ct));

    [HttpDelete("{contactId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(Guid contactId, CancellationToken ct)
    {
        await sender.Send(new DeleteNextOfKinContactCommand(contactId), ct);
        return NoContent();
    }
}
