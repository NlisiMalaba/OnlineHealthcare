using System.Text.Json;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;
using HealthPlatform.Application.Identity.UpdateDoctorProfile;

namespace HealthPlatform.API.Mapping;

public static class UpdateDoctorProfileCommandMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static UpdateDoctorProfileCommand ToCommand(UpdateDoctorProfileRequest request)
    {
        DoctorFileUpload? photo = null;
        if (request.Photo is { Length: > 0 })
        {
            photo = new DoctorFileUpload(
                request.Photo.OpenReadStream(),
                request.Photo.ContentType,
                request.Photo.FileName,
                request.Photo.Length);
        }

        DoctorFileUpload? credentials = null;
        if (request.Credentials is { Length: > 0 })
        {
            credentials = new DoctorFileUpload(
                request.Credentials.OpenReadStream(),
                request.Credentials.ContentType,
                request.Credentials.FileName,
                request.Credentials.Length);
        }

        IReadOnlyList<DoctorAvailabilitySlotInput>? availabilitySlots = null;
        if (!string.IsNullOrWhiteSpace(request.AvailabilitySlotsJson))
        {
            availabilitySlots = ParseAvailabilitySlots(request.AvailabilitySlotsJson);
        }

        return new UpdateDoctorProfileCommand(
            request.VirtualFee,
            request.PhysicalFee,
            request.Bio,
            availabilitySlots,
            photo,
            credentials);
    }

    private static IReadOnlyList<DoctorAvailabilitySlotInput> ParseAvailabilitySlots(string json)
    {
        var slots = JsonSerializer.Deserialize<List<DoctorAvailabilitySlotRequest>>(json, JsonOptions)
            ?? [];

        return slots
            .Select(slot => new DoctorAvailabilitySlotInput(
                slot.DayOfWeek,
                TimeOnly.Parse(slot.StartTime),
                TimeOnly.Parse(slot.EndTime),
                slot.SlotDurationMinutes,
                slot.AppointmentType))
            .ToList();
    }
}
