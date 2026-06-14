using System.Security.Claims;
using HealthPlatform.Application.Identity.UpdatePatientProfile;
using HealthPlatform.Application.Security;
using HealthPlatform.Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthPlatform.API.Controllers;

[ApiController]
[Route("api/v1/identity/patients/me")]
[Authorize(Policy = AuthorizationPolicies.Patient)]
public sealed class PatientProfileController(ISender sender) : ControllerBase
{
    [HttpPatch("profile")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(PatientProfileDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PatientProfileDto>> UpdateProfileAsync(
        [FromForm] UpdatePatientProfileRequest request,
        CancellationToken ct)
    {
        ProfilePhotoUpload? photo = null;
        if (request.Photo is { Length: > 0 })
        {
            photo = new ProfilePhotoUpload(
                request.Photo.OpenReadStream(),
                request.Photo.ContentType,
                request.Photo.FileName,
                request.Photo.Length);
        }

        var command = new UpdatePatientProfileCommand(
            request.FullName,
            request.DateOfBirth,
            request.BloodType,
            request.KnownAllergies,
            request.ChronicConditions,
            photo);

        return Ok(await sender.Send(command, ct));
    }

    public sealed class UpdatePatientProfileRequest
    {
        public string? FullName { get; init; }

        public DateOnly? DateOfBirth { get; init; }

        public BloodType? BloodType { get; init; }

        public List<string>? KnownAllergies { get; init; }

        public List<string>? ChronicConditions { get; init; }

        public IFormFile? Photo { get; init; }
    }
}
