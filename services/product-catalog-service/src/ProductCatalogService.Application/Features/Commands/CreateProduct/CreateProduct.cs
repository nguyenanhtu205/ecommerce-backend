using ValidationException = Common.Domain.Exceptions.ValidationException;

namespace ProductCatalogService.Application.Features.Commands.CreateProduct;

public record CreateProductCommand(
    string ShopId,
    string ShopName, // TODO: tạm nhận từ FE, sau này thay bằng tra shop_name_cache nội bộ
    string CategoryId,
    string Name,
    string Description,
    List<string> Tags,
    ProductCondition Condition,
    List<MediaAttachmentItem> MediaAttachments,
    string ThumbnailMediaId,
    string? VideoMediaId,
    List<string> GalleryMediaIds,
    List<SpecificationDto> Specifications,
    List<VariantGroupDto> VariantGroups,
    List<VariantCombinationDto> VariantCombinations,
    ShippingInfoDto ShippingInfo,
    bool IsPreOrder,
    int? PreOrderDays) : IRequest<ProductDto>;

public class CreateProduct(
    IApplicationDbContext context,
    ITopicProducer<ProductCreated> producer,
    ITopicProducer<ProductListingViewUpdated> listingViewProducer,
    ITopicProducer<ProductMediaAttached> mediaAttachedProducer
) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        Category category = await context.Categories
                                .Find(c => c.Id == request.CategoryId)
                                .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new NotFoundException($"Category '{request.CategoryId}' does not exist.");

        if (!category.IsLeaf)
        {
            throw new ValidationException([
                new ValidationFailure("Category", "Products can only be assigned to leaf categories.")
            ]);
        }

        string productId = Guid.CreateVersion7().ToString();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        List<VariantCombination> combinations =
        [
            .. request.VariantCombinations.Select(c => new VariantCombination
            {
                CombinationId =
                    string.IsNullOrEmpty(c.CombinationId) ? Guid.CreateVersion7().ToString() : c.CombinationId,
                OptionValues = c.OptionValues,
                Sku = c.Sku
            })
        ];

        Product product = new()
        {
            Id = productId,
            ShopId = request.ShopId,
            CategoryId = request.CategoryId,
            CategoryPath = category.Path,
            Name = request.Name,
            Description = request.Description,
            Tags = request.Tags,
            Condition = request.Condition,
            Status = ProductStatus.Draft,
            ThumbnailMediaId = request.ThumbnailMediaId,
            VideoMediaId = request.VideoMediaId,
            GalleryMediaIds = request.GalleryMediaIds,
            Specifications =
            [
                .. request.Specifications.Select(s => new Specification
                {
                    AttributeId = s.AttributeId, Title = s.Title, Value = s.Value
                })
            ],
            VariantGroups =
            [
                .. request.VariantGroups.Select(g => new VariantGroup
                {
                    Name = g.Name,
                    Options = [.. g.Options.Select(o => new VariantOption { Value = o.Value, MediaId = o.MediaId })]
                })
            ],
            VariantCombinations = combinations,
            ShippingInfo = new ShippingInfo
            {
                WeightGrams = request.ShippingInfo.WeightGrams,
                Dimensions = new Dimensions
                {
                    Length = request.ShippingInfo.Length,
                    Width = request.ShippingInfo.Width,
                    Height = request.ShippingInfo.Height
                }
            },
            IsPreOrder = request.IsPreOrder,
            PreOrderDays = request.PreOrderDays,
            CreatedAt = now,
            UpdatedAt = now
        };

        await context.Products.InsertOneAsync(product, cancellationToken: cancellationToken);

        string? brand = request.Specifications.FirstOrDefault(s => s.Title == "Thương hiệu")?.Value;
        string searchableSpecs = string.Join(", ", request.Specifications.Select(s => s.Value));
        int priceMin = request.VariantCombinations.Min(c => c.InitialPrice);
        int priceMax = request.VariantCombinations.Max(c => c.InitialPrice);
        int stockTotal = request.VariantCombinations.Sum(c => c.InitialStock);

        ProductListingView view = new()
        {
            Id = productId,
            ShopId = request.ShopId,
            ShopName = request.ShopName,
            Name = request.Name,
            Description = request.Description,
            Brand = brand,
            Tags = request.Tags,
            SearchableSpecs = searchableSpecs,
            ThumbnailUrl = request.ThumbnailMediaId,
            CategoryPath = category.Path,
            PriceMin = priceMin,
            PriceMax = priceMax,
            OriginalPriceMin = null,
            DiscountPercent = null,
            StockTotal = stockTotal,
            IsOutOfStock = stockTotal == 0,
            RatingAverage = 0,
            RatingCount = 0,
            SoldCount = 0,
            SyncedAt = now
        };

        await context.ProductListingViews.InsertOneAsync(view, cancellationToken: cancellationToken);

        await producer.Produce(new ProductCreated(
            productId,
            request.ShopId,
            [
                .. request.VariantCombinations.Select(c => new VariantCombinationInit(
                    combinations.First(x => x.OptionValues.SequenceEqual(c.OptionValues)).CombinationId,
                    c.Sku, c.InitialPrice, c.InitialStock))
            ],
            now), cancellationToken);

        await mediaAttachedProducer.Produce(new ProductMediaAttached(productId, request.ShopId,
            request.MediaAttachments, DateTimeOffset.UtcNow), cancellationToken);

        await listingViewProducer.Produce(new ProductListingViewUpdated(
            view.Id, view.ShopId, view.ShopName, view.Name, view.Description, view.Brand,
            view.Tags, view.SearchableSpecs, view.ThumbnailUrl, [
                .. view.CategoryPath
                    .Select(x => new CategoryPathItemEvent(x.Id, x.Name))
            ],
            view.PriceMin, view.PriceMax, view.OriginalPriceMin, view.DiscountPercent,
            view.StockTotal, view.IsOutOfStock, view.RatingAverage, view.RatingCount,
            view.SoldCount, view.SyncedAt), cancellationToken);

        return ProductMapper.ToDto(product);
    }
}
