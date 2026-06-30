using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Outbox;
using HealthPlatform.Domain.Payments;
using HealthPlatform.Domain.Payments.CreditLine;
using MediatR;

namespace HealthPlatform.Application.Payments.CreditLine.PayOnCreditLine;

public sealed class PayOnCreditLineCommandHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IPatientCreditLineRepository creditLineRepository,
    ICreditLineTransactionRepository transactionRepository,
    ICreditRepaymentReminderNotifier repaymentReminderNotifier,
    IPaymentCompletionService paymentCompletionService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher,
    TimeProvider timeProvider)
    : IRequestHandler<PayOnCreditLineCommand, CreditLinePaymentDto>
{
    public async Task<CreditLinePaymentDto> Handle(PayOnCreditLineCommand request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var creditLine = await creditLineRepository.GetByPatientIdAsync(patient.Id, ct)
            ?? throw new NotFoundException(
                CreditLineErrorCodes.CreditLineNotFound,
                "Patient does not have an active credit line.");

        if (!string.Equals(creditLine.Currency, request.Currency, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException(
                "CURRENCY_MISMATCH",
                "Credit line currency does not match the payment currency.");
        }

        var chargedAtUtc = timeProvider.GetUtcNow().UtcDateTime;

        CreditLineChargeResult chargeResult;
        try
        {
            chargeResult = creditLine.Charge(request.AmountMinorUnits, chargedAtUtc);
        }
        catch (CreditLimitExceededException)
        {
            throw new DomainException(
                CreditLineErrorCodes.CreditLimitExceeded,
                "The charge amount exceeds the patient's available credit line balance.");
        }

        var repaymentDueAtUtc = chargedAtUtc.AddDays(CreditLinePolicies.DefaultRepaymentDueDays);
        var transaction = CreditLineTransaction.RecordCharge(
            creditLine.Id,
            patient.Id,
            request.AmountMinorUnits,
            creditLine.Currency,
            chargeResult.NewOutstandingBalanceMinorUnits,
            request.AppointmentId,
            request.MedicationOrderId,
            request.LabOrderId,
            chargedAtUtc,
            repaymentDueAtUtc);

        await transactionRepository.AddAsync(transaction, ct);
        await creditLineRepository.UpdateAsync(creditLine, ct);

        await PublishDomainEventsAsync(creditLine, ct);
        creditLine.ClearDomainEvents();

        await repaymentReminderNotifier.NotifyRepaymentReminderAsync(
            patient.UserId,
            patient.Id,
            transaction.Id,
            request.AmountMinorUnits,
            chargeResult.NewOutstandingBalanceMinorUnits,
            creditLine.Currency,
            repaymentDueAtUtc,
            ct);

        transaction.MarkRepaymentReminderSent();
        await transactionRepository.UpdateAsync(transaction, ct);

        await paymentCompletionService.CompleteAsync(
            new CompletePaymentRequest(
                patient.Id,
                request.AmountMinorUnits,
                creditLine.Currency,
                PaymentMethod.CreditLine,
                PaymentGatewayType.Internal,
                transaction.Id.ToString(),
                request.AppointmentId,
                request.MedicationOrderId,
                request.LabOrderId,
                chargedAtUtc),
            ct);

        await creditLineRepository.SaveChangesAsync(ct);
        await transactionRepository.SaveChangesAsync(ct);

        return new CreditLinePaymentDto(
            transaction.Id,
            creditLine.Id,
            request.AmountMinorUnits,
            creditLine.Currency,
            chargeResult.NewOutstandingBalanceMinorUnits,
            creditLine.CreditLimitMinorUnits,
            repaymentDueAtUtc,
            chargeResult.BalanceWarningRequired);
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }

    private async Task PublishDomainEventsAsync(PatientCreditLine creditLine, CancellationToken ct)
    {
        foreach (var domainEvent in creditLine.DomainEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }
    }
}
