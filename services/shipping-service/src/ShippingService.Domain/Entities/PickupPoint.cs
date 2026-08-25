namespace ShippingService.Domain.Entities;

public class PickupPoint
{
    public required Guid Id { get; init; }

    public required Guid CarrierId { get; init; }

    public required string Name { get; init; }

    public required string Address { get; init; }

    public Carrier? Carrier { get; init; }
}
