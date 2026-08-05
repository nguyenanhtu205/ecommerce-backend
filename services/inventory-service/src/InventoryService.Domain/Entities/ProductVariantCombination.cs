namespace InventoryService.Domain.Entities;

public class ProductVariantCombination
{
    public required Guid Id { get; init; }

    public required Guid ProductId { get; init; }

    public required Guid ShopId { get; init; }

    public string? Sku { get; init; }

    public required int Price { get; init; }

    public required int Stock { get; init; }

    public required int ReservedStock { get; set; }

    public required int Version { get; set; }

    public required DateTimeOffset UpdatedAt { get; set; }

    public ICollection<StockReservation> StockReservations { get; private set; } = new List<StockReservation>();
}
