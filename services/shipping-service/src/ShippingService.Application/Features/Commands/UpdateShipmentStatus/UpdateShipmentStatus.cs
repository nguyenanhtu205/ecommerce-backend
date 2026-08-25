namespace ShippingService.Application.Features.Commands.UpdateShipmentStatus;

public record UpdateShipmentStatusCommand(string TrackingCode, ShipmentStatus NewStatus, string? Location)
    : IRequest;

public class UpdateShipmentStatus(IApplicationDbContext dbContext, IOutboxWriter outboxWriter)
    : IRequestHandler<UpdateShipmentStatusCommand>
{
    public async Task Handle(UpdateShipmentStatusCommand command, CancellationToken cancellationToken)
    {
        Shipment? shipment = await dbContext.Shipments
            .FirstOrDefaultAsync(s => s.TrackingCode == command.TrackingCode, cancellationToken);

        if (shipment is null || shipment.Status == command.NewStatus)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        shipment.Status = command.NewStatus;
        shipment.UpdatedAt = now;

        dbContext.ShipmentStatusHistories.Add(new ShipmentStatusHistory
        {
            ShipmentId = shipment.Id, Status = command.NewStatus, Location = command.Location, ChangedAt = now
        });

        if (command.NewStatus == ShipmentStatus.Delivered)
        {
            outboxWriter.Enqueue(new OrderDelivered(shipment.OrderId, now));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
