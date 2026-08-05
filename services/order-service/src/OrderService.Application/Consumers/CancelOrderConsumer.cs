namespace OrderService.Application.Consumers;

public class CancelOrderConsumer(IApplicationDbContext db) : IConsumer<CancelOrder>
{
    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        Order? order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == context.Message.OrderId, context.CancellationToken);

        if (order is null)
        {
            return;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return;
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Cancelled,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = context.Message.InitiatedBy
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
