namespace OrderService.Domain.Entities;

public class OrderItem : BaseEntity
{
    public required Guid OrderId { get; init; }

    public required Guid ProductId { get; init; }

    public required Guid CombinationId { get; init; }

    public required string ProductName { get; init; }

    public required string ThumbnailUrl { get; init; }

    public string? Variation { get; init; }

    public required int Quantity { get; init; }

    public required int Price { get; init; }

    public int? OriginalPrice { get; init; }

    public Order? Order { get; init; }

    public ICollection<OrderItemAddon> OrderItemAddons { get; private set; } = new List<OrderItemAddon>();
}
