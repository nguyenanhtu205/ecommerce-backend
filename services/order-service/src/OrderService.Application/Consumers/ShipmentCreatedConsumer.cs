namespace OrderService.Application.Consumers;

public class ShipmentCreatedConsumer(IApplicationDbContext dbContext) : IConsumer<ShipmentCreated>
{
    public async Task Consume(ConsumeContext<ShipmentCreated> context)
    {
        string eventId = context.MessageId?.ToString()
                         ?? throw new InvalidOperationException("ShipmentCreated message thiếu MessageId.");

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        Order? order = await dbContext.Orders
            .FirstOrDefaultAsync(o => o.Id == context.Message.OrderId, context.CancellationToken);

        if (order is null || order.Status == OrderStatus.Shipping)
        {
            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(ShipmentCreated),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        order.Status = OrderStatus.Shipping;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        dbContext.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            Status = OrderStatus.Shipping,
            ChangedAt = DateTimeOffset.UtcNow,
            ChangedBy = "system"
        });

        dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            EventType = nameof(ShipmentCreated),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
