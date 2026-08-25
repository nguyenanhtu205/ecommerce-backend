namespace OrderService.Application.Consumers;

public class OrderDeliveredConsumer(IApplicationDbContext dbContext, IOutboxWriter outboxWriter)
    : IConsumer<OrderDelivered>
{
    public async Task Consume(ConsumeContext<OrderDelivered> context)
    {
        string eventId = context.MessageId?.ToString()
                         ?? throw new InvalidOperationException("OrderDelivered message thiếu MessageId.");

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        Order? order = await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == context.Message.OrderId, context.CancellationToken);

        if (order is null)
        {
            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(OrderDelivered),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        if (order.Status == OrderStatus.Completed)
        {
            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(OrderDelivered),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        order.Status = OrderStatus.Completed;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Completed,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = "system"
        });

        OrderCompleted completedEvent = new(
            order.Id, order.ShopId, order.BuyerId, context.Message.DeliveredAt,
            [
                .. order.OrderItems.Select(i => new OrderCompletedItem(
                    i.Id, i.ProductId, i.CombinationId, i.Variation, i.Quantity))
            ]);

        outboxWriter.Enqueue(completedEvent);

        dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            EventType = nameof(OrderDelivered),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
