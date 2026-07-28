namespace ProductCatalogService.Application.Features.Commands.UpdateProduct;

public record UpdateProductCommand(
    string Id,
    string ShopId,
    string CategoryId,
    string Name,
    string Description,
    List<string> Tags,
    ProductCondition Condition,
    string Status,
    string ThumbnailMediaId,
    string? VideoMediaId,
    List<string> GalleryMediaIds,
    List<SpecificationDto> Specifications,
    List<VariantGroupDto> VariantGroups,
    List<VariantCombinationDto> VariantCombinations,
    ShippingInfoDto ShippingInfo,
    bool IsPreOrder,
    int? PreOrderDays
) : IRequest<ProductDto>;

public class UpdateProduct(
    IApplicationDbContext context,
    ITopicProducer<ProductCreated> producer,
    ITopicProducer<ProductListingViewUpdated> listingViewProducer) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        Product product = await context.Products
                              .Find(p => p.Id == request.Id)
                              .FirstOrDefaultAsync(cancellationToken)
                          ?? throw new NotFoundException($"Product '{request.Id}' does not exist.");

        if (product.ShopId != request.ShopId)
        {
            throw new ForbiddenAccessException("Shop does not have this product.");
        }

        List<CategoryPathItem> categoryPath = product.CategoryPath;
        if (product.CategoryId != request.CategoryId)
        {
            Category category = await context.Categories
                                    .Find(c => c.Id == request.CategoryId)
                                    .FirstOrDefaultAsync(cancellationToken)
                                ?? throw new NotFoundException($"Category '{request.CategoryId}' does not exist.");
            categoryPath = category.Path;
        }

        HashSet<string> existingCombinationIds = [.. product.VariantCombinations.Select(c => c.CombinationId)];

        product.CategoryId = request.CategoryId;
        product.CategoryPath = categoryPath;
        product.Name = request.Name;
        product.Description = request.Description;
        product.Tags = request.Tags;
        product.Condition = request.Condition;
        product.Status = Enum.Parse<ProductStatus>(request.Status, true);
        product.ThumbnailMediaId = request.ThumbnailMediaId;
        product.VideoMediaId = request.VideoMediaId;
        product.GalleryMediaIds = request.GalleryMediaIds;
        product.Specifications =
        [
            .. request.Specifications.Select(s =>
                new Specification { AttributeId = s.AttributeId, Title = s.Title, Value = s.Value })
        ];
        product.VariantGroups =
        [
            .. request.VariantGroups.Select(g => new VariantGroup
            {
                Name = g.Name,
                Options = [.. g.Options.Select(o => new VariantOption { Value = o.Value, MediaId = o.MediaId })]
            })
        ];
        product.VariantCombinations =
        [
            .. request.VariantCombinations.Select(c => new VariantCombination
            {
                CombinationId = string.IsNullOrEmpty(c.CombinationId) ? Guid.NewGuid().ToString() : c.CombinationId,
                OptionValues = c.OptionValues,
                Sku = c.Sku
            })
        ];
        product.ShippingInfo = new ShippingInfo
        {
            WeightGrams = request.ShippingInfo.WeightGrams,
            Dimensions = new Dimensions
            {
                Length = request.ShippingInfo.Length,
                Width = request.ShippingInfo.Width,
                Height = request.ShippingInfo.Height
            }
        };
        product.IsPreOrder = request.IsPreOrder;
        product.PreOrderDays = request.PreOrderDays;
        product.UpdatedAt = DateTimeOffset.UtcNow;

        await context.Products.ReplaceOneAsync(p => p.Id == product.Id, product,
            cancellationToken: cancellationToken);

        string? brand = request.Specifications.FirstOrDefault(s => s.Title == "Thương hiệu")?.Value;
        string searchableSpecs = string.Join(", ", request.Specifications.Select(s => s.Value));

        UpdateDefinition<ProductListingView> viewUpdate = Builders<ProductListingView>.Update
            .Set(v => v.Name, request.Name)
            .Set(v => v.Description, request.Description)
            .Set(v => v.Brand, brand)
            .Set(v => v.Tags, request.Tags)
            .Set(v => v.SearchableSpecs, searchableSpecs)
            .Set(v => v.ThumbnailUrl, request.ThumbnailMediaId)
            .Set(v => v.CategoryPath, categoryPath)
            .Set(v => v.SyncedAt, product.UpdatedAt);

        await context.ProductListingViews.UpdateOneAsync(v => v.Id == product.Id, viewUpdate,
            cancellationToken: cancellationToken);

        ProductListingView updatedView = await context.ProductListingViews
            .Find(v => v.Id == product.Id)
            .FirstAsync(cancellationToken);

        await listingViewProducer.Produce(new ProductListingViewUpdated(
            updatedView.Id, updatedView.ShopId, updatedView.ShopName, updatedView.Name,
            updatedView.Description, updatedView.Brand, updatedView.Tags, updatedView.SearchableSpecs,
            updatedView.ThumbnailUrl, [
                .. updatedView.CategoryPath
                    .Select(x => new CategoryPathItemEvent(x.Id, x.Name))
            ], updatedView.PriceMin, updatedView.PriceMax,
            updatedView.OriginalPriceMin, updatedView.DiscountPercent, updatedView.StockTotal,
            updatedView.IsOutOfStock, updatedView.RatingAverage, updatedView.RatingCount,
            updatedView.SoldCount, updatedView.SyncedAt), cancellationToken);

        List<VariantCombinationInit> newCombinations =
        [
            .. request.VariantCombinations
                .Where(c => !string.IsNullOrEmpty(c.CombinationId)
                            && !existingCombinationIds.Contains(c.CombinationId))
                .Select(c => new VariantCombinationInit(c.CombinationId!, c.Sku, c.InitialPrice, c.InitialStock))
        ];

        if (newCombinations.Count > 0)
        {
            await producer.Produce(new ProductCreated(product.Id, product.ShopId, newCombinations,
                product.UpdatedAt), cancellationToken);
        }

        return ProductMapper.ToDto(product);
    }
}
