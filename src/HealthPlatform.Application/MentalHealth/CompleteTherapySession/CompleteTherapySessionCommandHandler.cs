using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.HealthRecords;
using HealthPlatform.Application.Identity;
using HealthPlatform.Domain.MentalHealth;
using MediatR;

namespace HealthPlatform.Application.MentalHealth.CompleteTherapySession;

public sealed class CompleteTherapySessionCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IHealthRecordRepository healthRecordRepository,
    IHealthRecordEntryRepository healthRecordEntryRepository,
    ITherapySessionRepository therapySessionRepository,
    ITherapySessionSummaryRepository therapySessionSummaryRepository,
    TimeProvider timeProvider)
    : IRequestHandler<CompleteTherapySessionCommand, TherapySessionDto>
{
    public async Task<TherapySessionDto> Handle(CompleteTherapySessionCommand request, CancellationToken ct)
    {
        var therapist = await ResolveTherapistAsync(ct);
        var session = await therapySessionRepository.GetByIdAsync(request.TherapySessionId, ct)
            ?? throw new NotFoundException(
                TherapySessionErrorCodes.TherapySessionNotFound,
                "Therapy session was not found.");

        if (session.TherapistId != therapist.Id)
        {
            throw new AccessDeniedException(
                TherapySessionErrorCodes.TherapistAccessDenied,
                "Only the assigned therapist can complete this session.");
        }

        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(session.PatientId, ct)
            ?? throw new NotFoundException(
                TherapySessionErrorCodes.HealthRecordNotFound,
                "Patient health record was not found.");

        var completedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var summaryRecord = new TherapySessionSummaryRecord(
            session.Id,
            session.AppointmentId,
            session.PatientId,
            therapist.Id,
            request.SessionSummary.Trim(),
            completedAtUtc);

        var summaryReference = await therapySessionSummaryRepository.SaveAsync(summaryRecord, ct);
        var summaryEntry = await healthRecordEntryRepository.AddTherapySessionSummaryEntryAsync(
            new HealthRecordTherapySessionSummaryEntry(
                healthRecord.Id,
                session.PatientId,
                therapist.Id,
                session.Id,
                session.AppointmentId,
                summaryReference.DocumentId,
                completedAtUtc),
            ct);

        try
        {
            session.Complete(summaryReference.DocumentId, summaryEntry.EntryDocumentId, completedAtUtc);
        }
        catch (TherapySessionCompletionNotAllowedException ex)
        {
            throw new DomainException(TherapySessionErrorCodes.TherapySessionCompletionNotAllowed, ex.Message);
        }

        await therapySessionRepository.UpdateAsync(session, ct);
        return session.ToDto();
    }

    private async Task<Domain.Identity.Doctor> ResolveTherapistAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        return await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                TherapySessionErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");
    }
}
