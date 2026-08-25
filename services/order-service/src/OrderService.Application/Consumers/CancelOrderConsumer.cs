namespace OrderService.Application.Consumers;

public class CancelOrderConsumer(IApplicationDbContext db) : IConsumer<CancelOrder>
{
    public async Task Consume(ConsumeContext<CancelOrder> context)
    {
        string eventId = context.MessageId?.ToString()
                         ?? throw new InvalidOperationException("CancelOrder message thiếu MessageId.");

        if (await db.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        Order? order = await db.Orders
            .FirstOrDefaultAsync(o => o.Id == context.Message.OrderId, context.CancellationToken);

        if (order is null)
        {
            return;
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            db.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(CancelOrder),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync(context.CancellationToken);
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

        db.ProcessedEvents.Add(new ProcessedEvent
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            EventType = nameof(CancelOrder),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(context.CancellationToken);
    }
}
