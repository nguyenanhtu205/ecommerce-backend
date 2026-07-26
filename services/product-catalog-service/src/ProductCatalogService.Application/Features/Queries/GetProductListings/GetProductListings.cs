namespace ProductCatalogService.Application.Features.Queries.GetProductListings;

public enum ListingSortBy
{
    Newest,
    PriceAsc,
    PriceDesc,
    BestSelling,
    TopRated
}

public record GetProductListingsQuery(
    string? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Keyword,
    ListingSortBy SortBy,
    int Page,
    int PageSize) : IRequest<PagedResult<ProductListingDto>>;

public class GetProductListings(IApplicationDbContext context)
    : IRequestHandler<GetProductListingsQuery, PagedResult<ProductListingDto>>
{
    public async Task<PagedResult<ProductListingDto>> Handle(GetProductListingsQuery request,
        CancellationToken cancellationToken)
    {
        FilterDefinitionBuilder<ProductListingView> fb = Builders<ProductListingView>.Filter;
        FilterDefinition<ProductListingView> filter = fb.Empty;

        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            filter &= fb.ElemMatch(v => v.CategoryPath, p => p.Id == request.CategoryId);
        }

        if (request.MinPrice.HasValue)
        {
            filter &= fb.Gte(v => v.PriceMin, request.MinPrice.Value);
        }

        if (request.MaxPrice.HasValue)
        {
            filter &= fb.Lte(v => v.PriceMin, request.MaxPrice.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            filter &= fb.Regex(v => v.Name, new BsonRegularExpression(request.Keyword, "i"));
        }

        SortDefinition<ProductListingView> sort = request.SortBy switch
        {
            ListingSortBy.Newest => Builders<ProductListingView>.Sort.Descending(v => v.SyncedAt),
            ListingSortBy.PriceAsc => Builders<ProductListingView>.Sort.Ascending(v => v.PriceMin),
            ListingSortBy.PriceDesc => Builders<ProductListingView>.Sort.Descending(v => v.PriceMin),
            ListingSortBy.BestSelling => Builders<ProductListingView>.Sort.Descending(v => v.SoldCount),
            ListingSortBy.TopRated => Builders<ProductListingView>.Sort.Descending(v => v.RatingAverage),
            _ => Builders<ProductListingView>.Sort.Descending(v => v.SyncedAt)
        };

        long totalCount =
            await context.ProductListingViews.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        List<ProductListingView> items = await context.ProductListingViews
            .Find(filter)
            .Sort(sort)
            .Skip((request.Page - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListingDto>
        {
            Items = [.. items.Select(MapToDto)],
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    private static ProductListingDto MapToDto(ProductListingView v)
    {
        return new ProductListingDto
        {
            Id = v.Id,
            ShopId = v.ShopId,
            ShopName = v.ShopName,
            Name = v.Name,
            Brand = v.Brand,
            Tags = v.Tags,
            ThumbnailUrl = v.ThumbnailUrl,
            CategoryPath = [.. v.CategoryPath.Select(c => new CategoryPathItemDto { Id = c.Id, Name = c.Name })],
            PriceMin = v.PriceMin,
            PriceMax = v.PriceMax,
            OriginalPriceMin = v.OriginalPriceMin,
            DiscountPercent = v.DiscountPercent,
            IsOutOfStock = v.IsOutOfStock,
            RatingAverage = v.RatingAverage,
            RatingCount = v.RatingCount,
            SoldCount = v.SoldCount
        };
    }
}
