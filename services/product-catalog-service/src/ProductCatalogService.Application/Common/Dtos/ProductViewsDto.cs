namespace ProductCatalogService.Application.Common.Dtos;

public class ProductViewsDto
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string ThumbnailUrl { get; set; }

    public decimal PriceMin { get; set; }

    public decimal PriceMax { get; set; }

    public decimal? OriginalPriceMin { get; set; }

    public int? DiscountPercent { get; set; }

    public bool IsOutOfStock { get; set; }

    public int StockTotal { get; set; }

    public double RatingAverage { get; set; }

    public long RatingCount { get; set; }

    public long SoldCount { get; set; }

    public required string Location { get; set; }
}
