using HealthPlatform.Application.Outbox;
using HealthPlatform.Application.Storage;
using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Application.Payments;

public interface IPaymentCompletionService
{
    Task<PaymentCompletionResultDto> CompleteAsync(CompletePaymentRequest request, CancellationToken ct);
}

public sealed class PaymentCompletionService(
    IPaymentRepository paymentRepository,
    IPaymentReceiptGenerator receiptGenerator,
    IStorageService storageService,
    IOutboxRepository outboxRepository,
    IDomainEventPublisher domainEventPublisher)
    : IPaymentCompletionService
{
    public async Task<PaymentCompletionResultDto> CompleteAsync(CompletePaymentRequest request, CancellationToken ct)
    {
        var payment = Payment.RecordCompletion(
            request.PatientId,
            request.AmountMinorUnits,
            request.Currency,
            request.PaymentMethod,
            request.Gateway,
            request.GatewayReference,
            request.AppointmentId,
            request.MedicationOrderId,
            request.LabOrderId,
            request.CompletedAtUtc);

        var receiptBytes = receiptGenerator.Generate(payment);
        await using var receiptStream = new MemoryStream(receiptBytes);
        var upload = await storageService.UploadPaymentReceiptAsync(
            request.PatientId,
            payment.Id,
            receiptStream,
            ct);

        payment.AttachReceipt(upload.StorageKey);

        var pendingEvents = payment.DomainEvents.ToList();
        await paymentRepository.AddAsync(payment, ct);

        foreach (var domainEvent in pendingEvents)
        {
            await outboxRepository.EnqueueAsync(domainEvent, ct);
            await domainEventPublisher.PublishAsync(domainEvent, ct);
        }

        payment.ClearDomainEvents();
        await paymentRepository.SaveChangesAsync(ct);

        var receiptUrl = await storageService.GetSignedReadUrlAsync(upload.StorageKey, ct);
        return new PaymentCompletionResultDto(payment.Id, upload.StorageKey, receiptUrl);
    }
}
