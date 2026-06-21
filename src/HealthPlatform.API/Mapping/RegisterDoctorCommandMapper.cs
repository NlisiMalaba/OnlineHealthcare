using System.Text.Json;
using HealthPlatform.API.Requests.Identity;
using HealthPlatform.Application.Identity.RegisterDoctor;

namespace HealthPlatform.API.Mapping;

public static class RegisterDoctorCommandMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static RegisterDoctorCommand ToCommand(RegisterDoctorRequest request)
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

        return new RegisterDoctorCommand(
            request.FullName,
            request.LicenseNumber,
            request.Specialty,
            request.YearsOfExperience,
            request.ClinicAddress,
            request.ClinicLatitude,
            request.ClinicLongitude,
            request.VirtualFee,
            request.PhysicalFee,
            request.Bio,
            request.Email,
            request.PhoneNumber,
            request.Password,
            ParseAvailabilitySlots(request.AvailabilitySlotsJson),
            photo,
            credentials);
    }

    private static IReadOnlyList<DoctorAvailabilitySlotInput> ParseAvailabilitySlots(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

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
