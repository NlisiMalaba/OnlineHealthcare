using HealthPlatform.API.Mapping;
using HealthPlatform.API.Requests.Notifications;
using HealthPlatform.Application.Notifications;
using HealthPlatform.Application.Notifications.GetNotificationPreferences;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/notifications/preferences")]
[Authorize]
public sealed class NotificationPreferencesController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(NotificationPreferencesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationPreferencesDto>> GetAsync(CancellationToken ct) =>
        Ok(await sender.Send(new GetNotificationPreferencesQuery(), ct));

    [HttpPut]
    [ProducesResponseType(typeof(NotificationPreferencesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationPreferencesDto>> UpdateAsync(
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken ct) =>
        Ok(await sender.Send(NotificationPreferenceCommandMapper.ToCommand(request), ct));
}
