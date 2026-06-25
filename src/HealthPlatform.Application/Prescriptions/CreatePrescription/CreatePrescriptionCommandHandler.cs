using HealthPlatform.Application.Appointments;
using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Prescriptions.DrugInteractions;
using HealthPlatform.Application.Wellness;
using HealthPlatform.Domain.Identity;
using HealthPlatform.Domain.Prescriptions;
using HealthPlatform.Domain.Prescriptions.Events;
using MediatR;

namespace HealthPlatform.Application.Prescriptions.CreatePrescription;

public sealed class CreatePrescriptionCommandHandler(
    ICurrentUserAccessor currentUser,
    IDoctorRepository doctorRepository,
    IPatientRepository patientRepository,
    IHealthRecordRepository healthRecordRepository,
    IAppointmentRepository appointmentRepository,
    IMedicationScheduleRepository medicationScheduleRepository,
    IDrugInteractionChecker drugInteractionChecker,
    IPrescriptionRepository prescriptionRepository,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<CreatePrescriptionCommand, PrescriptionDto>
{
    public async Task<PrescriptionDto> Handle(CreatePrescriptionCommand request, CancellationToken ct)
    {
        var doctor = await ResolveVerifiedDoctorAsync(ct);
        var patient = await patientRepository.GetByIdAsync(request.PatientId, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.PatientNotFound,
                "Patient profile was not found.");

        var healthRecord = await healthRecordRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.HealthRecordNotFound,
                "Patient health record was not found.");

        await EnsureAppointmentLinkIsValidAsync(request.AppointmentId, doctor.Id, patient.Id, ct);
        await EmitDrugInteractionAlertsIfNeededAsync(
            doctor.Id,
            patient.Id,
            request.MedicationName,
            ct);

        var issuedAtUtc = timeProvider.GetUtcNow().UtcDateTime;
        var prescription = Prescription.Issue(
            doctor.Id,
            patient.Id,
            healthRecord.Id,
            request.MedicationName,
            request.Dosage,
            request.Frequency,
            request.DurationDays,
            request.SpecialInstructions,
            request.ExpiresAtUtc,
            request.AppointmentId,
            issuedAtUtc);

        await prescriptionRepository.AddAsync(prescription, ct);
        await PublishPendingEventsAsync(prescription, ct);

        return prescription.ToDto();
    }

    private async Task EmitDrugInteractionAlertsIfNeededAsync(
        Guid doctorId,
        Guid patientId,
        string proposedMedicationName,
        CancellationToken ct)
    {
        var activeSchedules = await medicationScheduleRepository.ListActiveByPatientIdAsync(patientId, ct);
        if (activeSchedules.Count == 0)
        {
            return;
        }

        var activeMedicationNames = activeSchedules
            .Select(schedule => schedule.MedicationName)
            .ToList();

        var interactions = drugInteractionChecker.Check(proposedMedicationName, activeMedicationNames);
        foreach (var interaction in interactions)
        {
            var alertEvent = new DrugInteractionAlertDetectedDomainEvent(
                doctorId,
                patientId,
                proposedMedicationName.Trim(),
                interaction.InteractingMedicationName,
                interaction.Description);

            await outboxRepository.EnqueueAsync(alertEvent, ct);
            await domainEventPublisher.PublishAsync(alertEvent, ct);
        }
    }

    private async Task<Doctor> ResolveVerifiedDoctorAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated user id is required.");

        var doctor = await doctorRepository.GetByUserIdWithSlotsAsync(userId, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.DoctorNotFound,
                "Doctor profile was not found.");

        if (doctor.VerificationStatus != DoctorVerificationStatus.Verified)
        {
            throw new DomainException(
                PrescriptionErrorCodes.DoctorNotVerified,
                "Only verified doctors can issue prescriptions.");
        }

        return doctor;
    }

    private async Task EnsureAppointmentLinkIsValidAsync(
        Guid? appointmentId,
        Guid doctorId,
        Guid patientId,
        CancellationToken ct)
    {
        if (!appointmentId.HasValue)
        {
            return;
        }

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId.Value, ct)
            ?? throw new NotFoundException(
                PrescriptionErrorCodes.AppointmentNotFound,
                "Appointment was not found.");

        if (appointment.DoctorId != doctorId || appointment.PatientId != patientId)
        {
            throw new AccessDeniedException(
                "ACCESS_DENIED",
                "Appointment does not belong to the prescribing doctor and patient.");
        }
    }

    private async Task PublishPendingEventsAsync(Prescription prescription, CancellationToken ct)
    {
        var pendingEvents = prescription.DomainEvents.ToList();
        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        prescription.ClearDomainEvents();
    }
}
