using OrderService.Application.Sagas.Checkout;

namespace OrderService.Application.Features.Queries.CheckoutStatus;

public record OrderStatusItem(
    Guid OrderId,
    string Status,
    int MerchandiseSubtotal,
    int ShippingFee,
    int VoucherDiscount,
    int TotalPayment);

public record CheckoutBatchStatusResult(
    Guid CheckoutBatchId,
    string SagaState,
    List<OrderStatusItem> Orders,
    int TotalAmount,
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
            .Select(o => new OrderStatusItem(
                o.Id, o.Status.ToString(), o.MerchandiseSubtotal, o.ShippingFee, o.VoucherDiscount, o.TotalPayment))
            .ToListAsync(cancellationToken);

        if (orders.Count == 0)
        {
            return null;
        }

        CheckoutSagaState? sagaState = await context.CheckoutSagaStates
            .FirstOrDefaultAsync(s => s.CorrelationId == query.CheckoutBatchId, cancellationToken);

        return new CheckoutBatchStatusResult(
            query.CheckoutBatchId, sagaState?.CurrentState ?? "Unknown", orders,
            orders.Sum(o => o.TotalPayment), sagaState?.RedirectUrl, sagaState?.FailReason);
    }
}
