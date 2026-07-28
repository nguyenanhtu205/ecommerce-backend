namespace InventoryService.Domain.Entities;

public class StockReservation : BaseEntity
{
    public required Guid CombinationId { get; init; }

    public required Guid OrderId { get; init; }

    public required int Quantity { get; init; }

    public required StockReservationStatus Status { get; set; }

    public required DateTimeOffset ExpiresAt { get; init; }

    public required DateTimeOffset CreatedAt { get; init; }

    public ProductVariantCombination? ProductVariantCombination { get; init; }
}
