namespace ProductCatalogService.Application.Common.Dtos;

public class ProductViewDto
{
    public required string Id { get; set; }

    public required string ShopId { get; set; }

    public required string ShopName { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public string? Brand { get; set; }

    public List<string> Tags { get; set; } = [];

    public required string Condition { get; set; }

    public List<ListingSpecificationDto> Specifications { get; set; } = [];

    public required string ThumbnailUrl { get; set; }

    public string? VideoUrl { get; set; }

    public List<string> GalleryUrls { get; set; } = [];

    public required string Location { get; set; }

    public List<CategoryPathItemDto> CategoryPath { get; set; } = [];

    public List<ListingVariantGroupDto> VariantGroups { get; set; } = [];

    public List<ListingVariantCombinationDto> VariantCombinations { get; set; } = [];

    public decimal PriceMin { get; set; }

    public decimal PriceMax { get; set; }

    public decimal? OriginalPriceMin { get; set; }

    public int? DiscountPercent { get; set; }

    public int StockTotal { get; set; }

    public bool IsOutOfStock { get; set; }

    public double RatingAverage { get; set; }

    public long RatingCount { get; set; }

    public long SoldCount { get; set; }

    public DateTimeOffset SyncedAt { get; set; }
}

public class ListingSpecificationDto
{
    public required string Title { get; init; }

    public required string Value { get; init; }
}

public class ListingVariantGroupDto
{
    public required string Name { get; init; }

    public List<ListingVariantOptionDto> Options { get; init; } = [];
}

public class ListingVariantOptionDto
{
    public required string Value { get; init; }

    public string? MediaId { get; init; }
}

public class ListingVariantCombinationDto
{
    public required string CombinationId { get; init; }

    public List<string> OptionValues { get; init; } = [];

    public required string Sku { get; init; }

    public int Price { get; init; }

    public int Stock { get; init; }
}
