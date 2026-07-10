using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.HealthRecords;
using HealthPlatform.Domain.MentalHealth;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.GrantTherapySessionBroaderAccess;

public sealed class GrantTherapySessionBroaderAccessCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    ITherapySessionRepository therapySessionRepository,
    TimeProvider timeProvider)
    : IRequestHandler<GrantTherapySessionBroaderAccessCommand, TherapySessionDto>
{
    public async Task<TherapySessionDto> Handle(
        GrantTherapySessionBroaderAccessCommand request,
        CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var session = await therapySessionRepository.GetByIdAsync(request.TherapySessionId, ct)
            ?? throw new NotFoundException(
                TherapySessionErrorCodes.TherapySessionNotFound,
                "Therapy session was not found.");

        if (session.PatientId != patient.Id)
        {
            throw new AccessDeniedException(
                TherapySessionErrorCodes.PatientAccessDenied,
                "Only the patient can grant broader access to this therapy session.");
        }

        var grantedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        try
        {
            session.GrantBroaderAccess(grantedAtUtc);
        }
        catch (TherapySessionBroaderAccessNotAllowedException ex)
        {
            throw new DomainException(
                TherapySessionErrorCodes.TherapySessionBroaderAccessNotAllowed,
                ex.Message);
        }

        if (!string.IsNullOrWhiteSpace(session.SummaryEntryId))
        {
            await healthRecordEntryRepository.UpdateAsync(
                new HealthRecordEntryUpdateModel(
                    session.SummaryEntryId,
                    new HealthRecordEntryContentPayload(
                        TherapySessionSummary: new TherapySessionSummaryContent(
                            session.Id,
                            session.AppointmentId,
                            session.TherapistId,
                            session.SummaryRef!,
                            BroaderAccessGranted: true)),
                    grantedAtUtc,
                    IsVisibleToPatient: null),
                ct);
        }

        await therapySessionRepository.UpdateAsync(session, ct);
        return session.ToDto();
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException(
                TherapySessionErrorCodes.PatientNotFound,
                "Patient profile was not found.");
    }
}
