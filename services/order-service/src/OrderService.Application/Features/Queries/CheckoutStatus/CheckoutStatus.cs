using OrderService.Application.Sagas.Checkout;

namespace OrderService.Application.Features.Queries.CheckoutStatus;

public record OrderStatusItem(Guid OrderId, string Status);

public record CheckoutBatchStatusResult(
    Guid CheckoutBatchId,
    string SagaState,
    List<OrderStatusItem> Orders,
    string? RedirectUrl,
    string? FailReason);

public record GetCheckoutBatchStatusQuery(Guid CheckoutBatchId) : IRequest<CheckoutBatchStatusResult?>;

public class GetCheckoutBatchStatusQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCheckoutBatchStatusQuery, CheckoutBatchStatusResult?>
{
    public async Task<CheckoutBatchStatusResult?> Handle(
        GetCheckoutBatchStatusQuery query, CancellationToken cancellationToken)
    {
        List<OrderStatusItem> orders = await context.Orders
            .Where(o => o.CheckoutBatchId == query.CheckoutBatchId)
            .Select(o => new OrderStatusItem(o.Id, o.Status.ToString()))
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return null;
        }

        CheckoutSagaState? sagaState = await context.CheckoutSagaStates
            .FirstOrDefaultAsync(s => s.CorrelationId == query.CheckoutBatchId, cancellationToken);

        return new CheckoutBatchStatusResult(
            query.CheckoutBatchId,
            sagaState?.CurrentState ?? "Unknown",
            orders,
            sagaState?.RedirectUrl,
            sagaState?.FailReason);
    }
}
