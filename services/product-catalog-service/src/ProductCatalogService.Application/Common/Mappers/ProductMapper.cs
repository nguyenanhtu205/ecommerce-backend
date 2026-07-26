namespace ProductCatalogService.Application.Common.Mappers;

public static class ProductMapper
{
    public static ProductDto ToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            ShopId = product.ShopId,
            CategoryId = product.CategoryId,
            CategoryPath =
                [.. product.CategoryPath.Select(c => new CategoryPathItemDto { Id = c.Id, Name = c.Name })],
            Name = product.Name,
            Description = product.Description,
            Tags = product.Tags,
            Condition = product.Condition.ToString(),
            Status = product.Status.ToString(),
            ThumbnailMediaId = product.ThumbnailMediaId,
            VideoMediaId = product.VideoMediaId,
            GalleryMediaIds = product.GalleryMediaIds,
            Specifications =
            [
                .. product.Specifications
                    .Select(s => new SpecificationDto { AttributeId = s.AttributeId, Title = s.Title, Value = s.Value })
            ],
            VariantGroups =
            [
                .. product.VariantGroups.Select(g => new VariantGroupDto
                {
                    Name = g.Name,
                    Options =
                        [.. g.Options.Select(o => new VariantOptionDto { Value = o.Value, MediaId = o.MediaId })]
                })
            ],
            VariantCombinations =
            [
                .. product.VariantCombinations.Select(c =>
                    new VariantCombinationDto
                    {
                        CombinationId = c.CombinationId, OptionValues = c.OptionValues, Sku = c.Sku
                    })
            ],
            ShippingInfo = new ShippingInfoDto
            {
                WeightGrams = product.ShippingInfo.WeightGrams,
                Length = product.ShippingInfo.Dimensions.Length,
                Width = product.ShippingInfo.Dimensions.Width,
                Height = product.ShippingInfo.Dimensions.Height
            },
            IsPreOrder = product.IsPreOrder,
            PreOrderDays = product.PreOrderDays,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }
}
