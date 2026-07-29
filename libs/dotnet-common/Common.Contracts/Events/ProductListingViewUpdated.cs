namespace Common.Contracts.Events;

public record ProductListingViewUpdated(
    string ProductId,
    string ShopId,
    string ShopName,
    string Name,
    string Description,
    string? Brand,
    List<string> Tags,
    string SearchableSpecs,
    string ThumbnailUrl,
    List<CategoryPathItemEvent> CategoryPath,
    decimal PriceMin,
    decimal PriceMax,
    decimal? OriginalPriceMin,
    int? DiscountPercent,
    int StockTotal,
    bool IsOutOfStock,
    double RatingAverage,
    long RatingCount,
    long SoldCount,
    DateTimeOffset SyncedAt
);

public record CategoryPathItemEvent(
    string Id,
    string Name
);
