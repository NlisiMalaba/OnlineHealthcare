using HealthPlatform.Application.Exceptions;
using HealthPlatform.Application.Identity;
using HealthPlatform.Application.Storage;
using MediatR;

namespace HealthPlatform.Application.Payments.GetPaymentReceipt;

public sealed class GetPaymentReceiptQueryHandler(
    ICurrentUserAccessor currentUser,
    IPatientRepository patientRepository,
    IPaymentRepository paymentRepository,
    IStorageService storageService)
    : IRequestHandler<GetPaymentReceiptQuery, PaymentReceiptDto>
{
    public async Task<PaymentReceiptDto> Handle(GetPaymentReceiptQuery request, CancellationToken ct)
    {
        var patient = await ResolvePatientAsync(ct);
        var payment = await paymentRepository.GetByIdForPatientAsync(request.PaymentId, patient.Id, ct)
            ?? throw new NotFoundException("PAYMENT_NOT_FOUND", "Payment was not found.");

        if (string.IsNullOrWhiteSpace(payment.ReceiptStorageKey))
        {
            throw new NotFoundException("RECEIPT_NOT_FOUND", "Payment receipt was not found.");
        }

        var receiptUrl = await storageService.GetSignedReadUrlAsync(payment.ReceiptStorageKey, ct);
        return new PaymentReceiptDto(payment.Id, receiptUrl);
    }

    private async Task<Domain.Identity.Patient> ResolvePatientAsync(CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new AccessDeniedException("ACCESS_DENIED", "Authenticated patient is required.");

        return await patientRepository.GetByUserIdAsync(userId, ct)
            ?? throw new NotFoundException("PATIENT_NOT_FOUND", "Patient profile was not found.");
    }
}
