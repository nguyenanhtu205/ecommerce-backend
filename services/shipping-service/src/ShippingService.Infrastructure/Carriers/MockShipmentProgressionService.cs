using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ShippingService.Application.Features.Commands.UpdateShipmentStatus;

namespace ShippingService.Infrastructure.Carriers;

public class MockShipmentProgressionService(IServiceScopeFactory scopeFactory, TimeProvider timeProvider)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            IApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            ISender sender = scope.ServiceProvider.GetRequiredService<ISender>();

            Carrier? carrier = await dbContext.Carriers.FirstOrDefaultAsync(c => c.Code == "mock", stoppingToken);
            if (carrier is not null)
            {
                List<Shipment> pendingShipments = await dbContext.Shipments
                    .Where(s => s.CarrierId == carrier.Id
                                && s.Status != ShipmentStatus.Delivered
                                && s.Status != ShipmentStatus.Failed
                                && s.UpdatedAt < timeProvider.GetUtcNow().AddSeconds(-60))
                    .ToListAsync(stoppingToken);

                foreach (Shipment shipment in pendingShipments)
                {
                    ShipmentStatus? nextStatus = shipment.Status switch
                    {
                        ShipmentStatus.Pending => ShipmentStatus.PickedUp,
                        ShipmentStatus.PickedUp => ShipmentStatus.InTransit,
                        ShipmentStatus.InTransit => ShipmentStatus.Delivered,
                        _ => null
                    };
                    if (nextStatus is not null)
                    {
                        await sender.Send(new UpdateShipmentStatusCommand(
                            shipment.TrackingCode!, nextStatus.Value, "Mock warehouse"), stoppingToken);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
