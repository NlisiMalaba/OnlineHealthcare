namespace HealthPlatform.Application.Payments;

public interface IPaymentGatewayResolver
{
    IPaymentGateway GetRequired(string providerName);

    IReadOnlyCollection<IPaymentGateway> GetAll();
}
