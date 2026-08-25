using ShippingService.Domain.Common;

namespace ShippingService.Application.Consumers;

public class CreateShipmentConsumer(
    IApplicationDbContext dbContext,
    ICarrierAdapterFactory adapterFactory,
    IOutboxWriter outboxWriter) : IConsumer<CreateShipment>
{
    private const int DefaultWeightGram = 500;

    public async Task Consume(ConsumeContext<CreateShipment> context)
    {
        string eventId = $"{nameof(CreateShipment)}-{context.Message.OrderId}";

        if (await dbContext.ProcessedEvents.AnyAsync(e => e.EventId == eventId, context.CancellationToken))
        {
            return;
        }

        bool exists = await dbContext.Shipments
            .AnyAsync(s => s.OrderId == context.Message.OrderId, context.CancellationToken);
        if (exists)
        {
            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(CreateShipment),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        Carrier? carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.Id == context.Message.CarrierId, context.CancellationToken);

        if (carrier is null)
        {
            outboxWriter.Enqueue(new ShipmentCreationFailed(context.Message.OrderId,
                $"Carrier {context.Message.CarrierId} does not exist."));

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(CreateShipment),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        AddressSnapshot pickup = AddressMapper.ToDomain(context.Message.PickupAddressSnapshot);
        AddressSnapshot delivery = AddressMapper.ToDomain(context.Message.DeliveryAddressSnapshot);
        ICarrierShippingAdapter adapter = adapterFactory.GetAdapter(carrier.Code);

        CarrierCreateOrderResult orderResult = await adapter.CreateOrderAsync(
            new CarrierCreateOrderRequest(context.Message.OrderId, pickup, delivery, DefaultWeightGram, 0, null),
            context.CancellationToken);

        if (!orderResult.Success)
        {
            outboxWriter.Enqueue(new ShipmentCreationFailed(context.Message.OrderId,
                orderResult.FailureReason ?? "Carrier create order failed"));

            dbContext.ProcessedEvents.Add(new ProcessedEvent
            {
                Id = Guid.CreateVersion7(),
                EventId = eventId,
                EventType = nameof(CreateShipment),
                ProcessedAt = DateTimeOffset.UtcNow
            });
            await dbContext.SaveChangesAsync(context.CancellationToken);
            return;
        }

        dbContext.Shipments.Add(new Shipment
        {
            OrderId = context.Message.OrderId,
            CarrierId = context.Message.CarrierId,
            TrackingCode = orderResult.TrackingCode,
            Status = ShipmentStatus.Pending,
            PickupAddressSnapshot = pickup,
            DeliveryAddressSnapshot = delivery,
            EstimatedDeliveryStart = orderResult.EstimatedStart,
            EstimatedDeliveryEnd = orderResult.EstimatedEnd,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        });

        outboxWriter.Enqueue(new ShipmentCreated(context.Message.OrderId));

        dbContext.ProcessedEvents.Add(new ProcessedEvent
        {
            Id = Guid.CreateVersion7(),
            EventId = eventId,
            EventType = nameof(CreateShipment),
            ProcessedAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
    }
}
