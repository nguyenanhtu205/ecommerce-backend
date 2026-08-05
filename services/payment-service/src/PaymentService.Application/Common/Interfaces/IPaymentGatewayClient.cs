namespace PaymentService.Application.Common.Interfaces;

public interface IPaymentGatewayClient
{
    Task<string> CreateRedirectUrlAsync(Guid checkoutBatchId, int amount, CancellationToken cancellationToken);
}
