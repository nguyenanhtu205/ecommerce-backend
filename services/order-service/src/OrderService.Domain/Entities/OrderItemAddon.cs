namespace OrderService.Domain.Entities;

public class OrderItemAddon : BaseEntity
{
    public required Guid OrderItemId { get; init; }

    public required string Label { get; init; }

    public required int Price { get; init; }

    public OrderItem? OrderItem { get; init; }
}
