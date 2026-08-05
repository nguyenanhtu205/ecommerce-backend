using ShippingService.Domain.Common;

namespace ShippingService.Application.Consumers;

public class CreateShipmentConsumer(
    IApplicationDbContext dbContext,
    TimeProvider timeProvider,
    ITopicProducer<ShipmentCreated> shipmentCreatedProducer,
    ITopicProducer<ShipmentCreationFailed> shipmentCreationFailedProducer) : IConsumer<CreateShipment>
{
    public async Task Consume(ConsumeContext<CreateShipment> context)
    {
        bool exists = await dbContext.Shipments
            .AnyAsync(s => s.OrderId == context.Message.OrderId, context.CancellationToken);
        if (exists)
        {
            return;
        }

        Carrier? carrier = await dbContext.Carriers
            .FirstOrDefaultAsync(c => c.Id == context.Message.CarrierId, context.CancellationToken);

        if (carrier is null)
        {
            await shipmentCreationFailedProducer.Produce(
                new ShipmentCreationFailed(
                    context.Message.OrderId, $"Carrier {context.Message.CarrierId} does not exist."),
                context.CancellationToken);
            return;
        }

        dbContext.Shipments.Add(new Shipment
        {
            OrderId = context.Message.OrderId,
            CarrierId = context.Message.CarrierId,
            Status = ShipmentStatus.Pending,
            PickupAddressSnapshot =
                new AddressSnapshot
                {
                    AddressDetail = context.Message.PickupAddressSnapshot.AddressDetail,
                    AddressType = context.Message.PickupAddressSnapshot.AddressType,
                    FullAddressText = context.Message.PickupAddressSnapshot.FullAddressText,
                    FullName = context.Message.PickupAddressSnapshot.FullName,
                    Latitude = context.Message.PickupAddressSnapshot.Latitude,
                    Longitude = context.Message.PickupAddressSnapshot.Longitude,
                    Phone = context.Message.PickupAddressSnapshot.Phone,
                    Province = context.Message.PickupAddressSnapshot.Province,
                    UserId = context.Message.PickupAddressSnapshot.UserId,
                    Ward = context.Message.PickupAddressSnapshot.Ward
                },
            DeliveryAddressSnapshot = new AddressSnapshot
            {
                AddressDetail = context.Message.DeliveryAddressSnapshot.AddressDetail,
                AddressType = context.Message.DeliveryAddressSnapshot.AddressType,
                FullAddressText = context.Message.DeliveryAddressSnapshot.FullAddressText,
                FullName = context.Message.DeliveryAddressSnapshot.FullName,
                Latitude = context.Message.DeliveryAddressSnapshot.Latitude,
                Longitude = context.Message.DeliveryAddressSnapshot.Longitude,
                Phone = context.Message.DeliveryAddressSnapshot.Phone,
                Province = context.Message.DeliveryAddressSnapshot.Province,
                UserId = context.Message.DeliveryAddressSnapshot.UserId,
                Ward = context.Message.DeliveryAddressSnapshot.Ward
            },
            CreatedAt = timeProvider.GetUtcNow().UtcDateTime,
            UpdatedAt = timeProvider.GetUtcNow().UtcDateTime
        });

        await dbContext.SaveChangesAsync(context.CancellationToken);
        await shipmentCreatedProducer.Produce(
            new ShipmentCreated(context.Message.OrderId), context.CancellationToken);
    }
}
