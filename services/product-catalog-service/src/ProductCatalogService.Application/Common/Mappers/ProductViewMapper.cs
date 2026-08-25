namespace ProductCatalogService.Application.Common.Mappers;

public static class ProductViewMapper
{
    public static ProductViewDto ToDto(ProductListingView view, Product product)
    {
        return new ProductViewDto
        {
            Id = view.Id,
            ShopId = view.ShopId,
            ShopName = view.ShopName,
            Name = view.Name,
            Description = view.Description,
            Brand = view.Brand,
            Tags = view.Tags,
            Condition = view.Condition.ToString(),
            Specifications =
            [
                .. view.Specifications.Select(s => new ListingSpecificationDto { Title = s.Title, Value = s.Value })
            ],
            ThumbnailUrl = view.ThumbnailUrl,
            VideoUrl = view.VideoUrl,
            GalleryUrls = view.GalleryUrls,
            Location = view.Location,
            CategoryPath =
            [
                .. view.CategoryPath.Select(c => new CategoryPathItemDto { Id = c.Id, Slug = c.Slug, Name = c.Name })
            ],
            VariantGroups =
            [
                .. view.VariantGroups.Select(g => new ListingVariantGroupDto
                {
                    Name = g.Name,
                    Options =
                    [
                        .. g.Options.Select(o => new ListingVariantOptionDto { Value = o.Value, MediaId = o.MediaId })
                    ]
                })
            ],
            VariantCombinations =
            [
                .. view.VariantCombinations.Select(c => new ListingVariantCombinationDto
                {
                    CombinationId = c.CombinationId,
                    OptionValues = c.OptionValues,
                    Sku = c.Sku,
                    Price = c.Price,
                    Stock = c.Stock
                })
            ],
            PriceMin = view.PriceMin,
            PriceMax = view.PriceMax,
            OriginalPriceMin = view.OriginalPriceMin,
            DiscountPercent = view.DiscountPercent,
            StockTotal = view.StockTotal,
            IsOutOfStock = view.IsOutOfStock,
            RatingAverage = view.RatingAverage,
            RatingCount = view.RatingCount,
            SoldCount = view.SoldCount,
            SyncedAt = view.SyncedAt,
            IsPreOrder = view.IsPreOrder,
            PreOrderDays = view.PreOrderDays,
            ShippingInfo = product.ShippingInfo
        };
    }
}
