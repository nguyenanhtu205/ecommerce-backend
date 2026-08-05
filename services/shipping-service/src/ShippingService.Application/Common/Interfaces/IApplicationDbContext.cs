namespace ShippingService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Carrier> Carriers { get; }

    DbSet<PickupPoint> PickupPoints { get; }
    
    DbSet<Shipment> Shipments { get; }
    
    DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
