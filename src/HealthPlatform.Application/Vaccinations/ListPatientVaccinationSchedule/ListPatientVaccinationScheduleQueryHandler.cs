using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Security;
using HealthPlatform.Application.Vaccinations;
using MediatR;

namespace HealthPlatform.Application.Vaccinations.ListPatientVaccinationSchedule;

public sealed class ListPatientVaccinationScheduleQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IVaccinationScheduleRepository scheduleRepository,
    IVaccinationScheduleInitializer scheduleInitializer,
    TimeProvider timeProvider)
    : IRequestHandler<ListPatientVaccinationScheduleQuery, IReadOnlyList<VaccinationScheduleEntryDto>>
{
    public async Task<IReadOnlyList<VaccinationScheduleEntryDto>> Handle(
        ListPatientVaccinationScheduleQuery request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var now = timeProvider.GetUtcNow().UtcDateTime;

        if (!await scheduleRepository.HasScheduleForPatientAsync(patient.Id, ct))
        {
            await scheduleInitializer.InitializePatientScheduleAsync(patient, now, ct);
        }

        var entries = await scheduleRepository.ListByPatientIdAsync(patient.Id, ct);
        return entries.Select(entry => entry.ToDto()).ToList();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                VaccinationErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
