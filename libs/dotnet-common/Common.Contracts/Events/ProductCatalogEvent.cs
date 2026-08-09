namespace Common.Contracts.Events;

public record VariantCombinationInit(string CombinationId, string Sku, int InitialPrice, int InitialStock);

public record ProductCreated(
    string ProductId,
    string ShopId,
    List<VariantCombinationInit> VariantCombinations,
    DateTimeOffset CreatedAt);

public record CategoryPathItemEvent(string Id, string Name);

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
    string Location,
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

public record MediaAttachmentItem(string MediaAssetId, string Role, int Position);

public record ProductMediaAttached(
    string ProductId,
    string ShopId,
    List<MediaAttachmentItem> MediaAttachments,
    DateTimeOffset OccurredAt);
