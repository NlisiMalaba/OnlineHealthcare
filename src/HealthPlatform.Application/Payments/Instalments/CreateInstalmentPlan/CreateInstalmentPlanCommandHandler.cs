using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Payments.Instalments;
using MediatR;
using Microsoft.Extensions.Options;

namespace HealthPlatform.Application.Payments.Instalments.CreateInstalmentPlan;

public sealed class CreateInstalmentPlanCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IInstalmentPlanRepository planRepository,
    IInstalmentPaymentRepository paymentRepository,
    IPaymentCompletionService paymentCompletionService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    IOptions<InstalmentPlanOptions> options,
    TimeProvider timeProvider)
    : IRequestHandler<CreateInstalmentPlanCommand, InstalmentPlanDto>
{
    public async Task<InstalmentPlanDto> Handle(CreateInstalmentPlanCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var settings = options.Value;

        if (request.InstalmentCount > settings.MaxInstalments)
        {
            throw new DomainException(
                InstalmentErrorCodes.InvalidInstalmentPlan,
                $"Instalment count cannot exceed {settings.MaxInstalments}.");
        }

        InstalmentPlan plan;
        try
        {
            plan = InstalmentPlan.Create(
                patient.Id,
                request.TotalAmountMinorUnits,
                request.Frequency,
                request.InstalmentCount,
                request.Currency,
                request.FirstDueDate,
                request.AppointmentId,
                request.MedicationOrderId,
                request.LabOrderId,
                settings.MinimumExpenseMinorUnits);
        }
        catch (InstalmentExpenseBelowThresholdException)
        {
            throw new DomainException(
                InstalmentErrorCodes.ExpenseBelowThreshold,
                $"Healthcare expense must be at least {settings.MinimumExpenseMinorUnits} minor units to qualify for an instalment plan.");
        }
        catch (InvalidInstalmentPlanException ex)
        {
            throw new DomainException(InstalmentErrorCodes.InvalidInstalmentPlan, ex.Message);
        }

        var schedule = InstalmentPolicies.BuildSchedule(
            request.TotalAmountMinorUnits,
            request.InstalmentCount,
            request.Frequency,
            request.FirstDueDate);

        var payments = schedule
            .Select(entry => InstalmentPayment.Schedule(plan.Id, patient.Id, entry, plan.Currency))
            .ToList();

        await planRepository.AddAsync(plan, ct);
        await paymentRepository.AddRangeAsync(payments, ct);

        foreach (var domainEvent in plan.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        plan.ClearDomainEvents();

        await paymentCompletionService.CompleteAsync(
            new CompletePaymentRequest(
                patient.Id,
                request.TotalAmountMinorUnits,
                plan.Currency,
                PaymentMethod.Instalment,
                PaymentGatewayType.Internal,
                plan.Id.ToString(),
                request.AppointmentId,
                request.MedicationOrderId,
                request.LabOrderId,
                timeProvider.GetUtcNow().UtcDateTime),
            ct);

        await planRepository.SaveChangesAsync(ct);
        await paymentRepository.SaveChangesAsync(ct);

        return plan.ToDto(payments);
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
