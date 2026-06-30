using HealthPlatform.Domain.Payments;

namespace HealthPlatform.Application.Payments;

public interface IPaymentReceiptGenerator
{
    byte[] Generate(Payment payment);
}
