namespace ProductCatalogService.Domain.Entities;

public class ProductListingView
{
    public required string Id { get; init; }

    public required string ShopId { get; init; }

    public required string ShopName { get; init; }

    public required string Name { get; init; }

    public required string Description { get; set; }

    public string? Brand { get; init; }

    public List<string> Tags { get; init; } = [];

    public required string SearchableSpecs { get; set; }

    public required string ThumbnailUrl { get; init; }

    public required string Location { get; init; }

    public List<CategoryPathItem> CategoryPath { get; init; } = [];

    public decimal PriceMin { get; init; }

    public decimal PriceMax { get; init; }

    public decimal? OriginalPriceMin { get; init; }

    public int? DiscountPercent { get; init; }

    public required int StockTotal { get; set; }

    public required bool IsOutOfStock { get; init; }

    public required double RatingAverage { get; init; }

    public required long RatingCount { get; init; }

    public required long SoldCount { get; init; }

    public required DateTimeOffset SyncedAt { get; init; }
}
