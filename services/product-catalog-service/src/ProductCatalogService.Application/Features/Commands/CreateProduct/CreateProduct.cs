using Common.Application.Interfaces;
using ValidationException = Common.Domain.Exceptions.ValidationException;

namespace ProductCatalogService.Application.Features.Commands.CreateProduct;

public record CreateProductCommand(
    string CategoryId,
    string Name,
    string Description,
    List<string> Tags,
    ProductCondition Condition,
    List<MediaAttachmentItem> MediaAttachments,
    string ThumbnailMediaId,
    string Location,
    string? VideoMediaId,
    List<string> GalleryMediaIds,
    List<SpecificationDto> Specifications,
    List<VariantGroupDto> VariantGroups,
    List<VariantCombinationDto> VariantCombinations,
    ShippingInfoDto ShippingInfo,
    bool IsPreOrder,
    int? PreOrderDays) : IRequest<ProductDto>;

public class CreateProduct(
    ICurrentUser currentUser,
    IApplicationDbContext context,
    ITopicProducer<ProductCreated> producer,
    ITopicProducer<ProductListingViewUpdated> listingViewProducer,
    ITopicProducer<ProductMediaAttached> mediaAttachedProducer
) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId == null)
        {
            throw new ForbiddenAccessException();
        }

        Guid shopId = currentUser.ShopId.Value;

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
            ShopId = shopId.ToString(),
            CategoryId = request.CategoryId,
            CategoryPath = category.Path,
            Name = request.Name,
            Description = request.Description,
            Tags = request.Tags,
            Condition = request.Condition,
            Status = ProductStatus.Active,
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
            ShopId = shopId.ToString(),
            ShopName = currentUser.ShopName ?? "Unknown",
            Name = request.Name,
            Description = request.Description,
            Brand = brand,
            Tags = request.Tags,
            Condition = request.Condition,
            SearchableSpecs = searchableSpecs,
            Specifications =
            [
                .. request.Specifications.Select(s => new ListingSpecification { Title = s.Title, Value = s.Value })
            ],
            ThumbnailUrl = request.ThumbnailMediaId,
            VideoUrl = request.VideoMediaId,
            GalleryUrls = request.GalleryMediaIds,
            Location = request.Location,
            CategoryPath = category.Path,
            VariantGroups =
            [
                .. request.VariantGroups.Select(g => new ListingVariantGroup
                {
                    Name = g.Name,
                    Options =
                    [
                        .. g.Options.Select(o => new ListingVariantOption { Value = o.Value, MediaId = o.MediaId })
                    ]
                })
            ],
            VariantCombinations =
            [
                .. request.VariantCombinations.Select(c => new ListingVariantCombination
                {
                    CombinationId =
                        combinations.First(x => x.OptionValues.SequenceEqual(c.OptionValues)).CombinationId,
                    OptionValues = c.OptionValues,
                    Sku = c.Sku,
                    Price = c.InitialPrice,
                    Stock = c.InitialStock
                })
            ],
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
            shopId.ToString(),
            [
                .. request.VariantCombinations.Select(c => new VariantCombinationInit(
                    combinations.First(x => x.OptionValues.SequenceEqual(c.OptionValues)).CombinationId,
                    c.Sku, c.InitialPrice, c.InitialStock))
            ],
            now), cancellationToken);

        await mediaAttachedProducer.Produce(new ProductMediaAttached(productId, shopId.ToString(),
            request.MediaAttachments, DateTimeOffset.UtcNow), cancellationToken);

        await listingViewProducer.Produce(new ProductListingViewUpdated(
            view.Id, view.ShopId, view.ShopName, view.Name, view.Description, view.Brand,
            view.Tags, view.SearchableSpecs, view.ThumbnailUrl, view.Location, [
                .. view.CategoryPath
                    .Select(x => new CategoryPathItemEvent(x.Id, x.Name))
            ],
            view.PriceMin, view.PriceMax, view.OriginalPriceMin, view.DiscountPercent,
            view.StockTotal, view.IsOutOfStock, view.RatingAverage, view.RatingCount,
            view.SoldCount, view.SyncedAt), cancellationToken);

        return ProductMapper.ToDto(product);
    }
}
