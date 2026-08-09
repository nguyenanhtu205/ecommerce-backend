namespace ProductCatalogService.Application.Common.Dtos;

public class ProductListingDto
{
    public required string Id { get; set; }

    public required string ShopId { get; set; }

    public required string ShopName { get; set; }

    public required string Name { get; set; }

    public string? Brand { get; set; }

    public List<string> Tags { get; set; } = [];

    public required string ThumbnailUrl { get; set; }

    public required string Location { get; init; }

    public List<CategoryPathItemDto> CategoryPath { get; set; } = [];

    public decimal PriceMin { get; set; }

    public decimal PriceMax { get; set; }

    public decimal? OriginalPriceMin { get; set; }

    public int? DiscountPercent { get; set; }

    public bool IsOutOfStock { get; set; }

    public double RatingAverage { get; set; }

    public long RatingCount { get; set; }

    public long SoldCount { get; set; }
}
