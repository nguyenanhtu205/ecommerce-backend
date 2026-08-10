namespace ProductCatalogService.Domain.Entities;

public class ProductListingView
{
    public required string Id { get; init; }

    public required string ShopId { get; init; }

    public required string ShopName { get; init; }

    public required string Name { get; init; }

    public required string Description { get; init; }

    public string? Brand { get; init; }

    public List<string> Tags { get; init; } = [];

    public required ProductCondition Condition { get; init; }

    public required string SearchableSpecs { get; init; }

    public List<ListingSpecification> Specifications { get; init; } = [];

    public required string ThumbnailUrl { get; init; }

    public string? VideoUrl { get; init; }

    public List<string> GalleryUrls { get; init; } = [];

    public required string Location { get; init; }

    public List<CategoryPathItem> CategoryPath { get; init; } = [];

    public List<ListingVariantGroup> VariantGroups { get; init; } = [];

    public List<ListingVariantCombination> VariantCombinations { get; set; } = [];

    public decimal PriceMin { get; set; }

    public decimal PriceMax { get; set; }

    public decimal? OriginalPriceMin { get; init; }

    public int? DiscountPercent { get; init; }

    public required int StockTotal { get; set; }

    public required bool IsOutOfStock { get; set; }

    public required double RatingAverage { get; init; }

    public required long RatingCount { get; init; }

    public required long SoldCount { get; init; }

    public required DateTimeOffset SyncedAt { get; set; }
}

public class ListingSpecification
{
    public required string Title { get; init; }

    public required string Value { get; init; }
}

public class ListingVariantGroup
{
    public required string Name { get; init; }

    public List<ListingVariantOption> Options { get; init; } = [];
}

public class ListingVariantOption
{
    public required string Value { get; init; }

    public string? MediaId { get; init; }
}

public class ListingVariantCombination
{
    public required string CombinationId { get; init; }

    public List<string> OptionValues { get; init; } = [];

    public required string Sku { get; init; }

    public required int Price { get; set; }

    public required int Stock { get; set; }
}
