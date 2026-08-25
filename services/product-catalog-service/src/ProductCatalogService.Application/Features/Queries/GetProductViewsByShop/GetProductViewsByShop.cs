using System.Linq.Expressions;

namespace ProductCatalogService.Application.Features.Queries.GetProductViewsByShop;

public enum ProductSortBy
{
    Newest = 0,
    PriceAsc = 1,
    PriceDesc = 2,
    RatingAsc = 3,
    RatingDesc = 4,
    BestSelling = 5
}

public record GetProductViewsByShopQuery(
    string ShopId,
    string? ProductId,
    string? CategoryId,
    ProductSortBy? SortBy,
    int? Page,
    int? PageSize) : IRequest<PagedResult<ProductViewsDto>>;

public class GetProductViewsByShop(IApplicationDbContext context)
    : IRequestHandler<GetProductViewsByShopQuery, PagedResult<ProductViewsDto>>
{
    private const int SuggestedLimit = 5;
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private static readonly Expression<Func<ProductListingView, ProductViewsDto>> Projection = p => new ProductViewsDto
    {
        Id = p.Id,
        Name = p.Name,
        ThumbnailUrl = p.ThumbnailUrl,
        Location = p.Location,
        PriceMin = p.PriceMin,
        PriceMax = p.PriceMax,
        OriginalPriceMin = p.OriginalPriceMin,
        DiscountPercent = p.DiscountPercent,
        IsOutOfStock = p.IsOutOfStock,
        StockTotal = p.StockTotal,
        RatingAverage = p.RatingAverage,
        RatingCount = p.RatingCount,
        SoldCount = p.SoldCount
    };

    public async Task<PagedResult<ProductViewsDto>> Handle(GetProductViewsByShopQuery request,
        CancellationToken cancellationToken)
    {
        FilterDefinitionBuilder<ProductListingView> fb = Builders<ProductListingView>.Filter;
        FilterDefinition<ProductListingView> filter = fb.Eq(p => p.ShopId, request.ShopId);
        
        if (!string.IsNullOrWhiteSpace(request.ProductId))
        {
            filter &= fb.Ne(p => p.Id, request.ProductId);

            List<ProductViewsDto> suggested = await context.ProductListingViews
                .Find(filter)
                .SortByDescending(p => p.SoldCount)
                .Limit(SuggestedLimit)
                .Project(Projection)
                .ToListAsync(cancellationToken);

            return new PagedResult<ProductViewsDto>
            {
                Items = suggested, Page = 1, PageSize = SuggestedLimit, TotalCount = suggested.Count
            };
        }
        
        if (!string.IsNullOrWhiteSpace(request.CategoryId))
        {
            filter &= fb.ElemMatch(p => p.CategoryPath, cp => cp.Id == request.CategoryId);
        }

        int page = request.Page is > 0 ? request.Page.Value : DefaultPage;
        int pageSize = request.PageSize switch
        {
            > 0 and <= MaxPageSize => request.PageSize.Value,
            > MaxPageSize => MaxPageSize,
            _ => DefaultPageSize
        };

        SortDefinition<ProductListingView> sort = request.SortBy switch
        {
            ProductSortBy.PriceAsc => Builders<ProductListingView>.Sort.Ascending(p => p.PriceMin),
            ProductSortBy.PriceDesc => Builders<ProductListingView>.Sort.Descending(p => p.PriceMin),
            ProductSortBy.RatingAsc => Builders<ProductListingView>.Sort.Ascending(p => p.RatingAverage),
            ProductSortBy.RatingDesc => Builders<ProductListingView>.Sort.Descending(p => p.RatingAverage),
            ProductSortBy.BestSelling => Builders<ProductListingView>.Sort.Descending(p => p.SoldCount),
            _ => Builders<ProductListingView>.Sort.Descending(p => p.SyncedAt)
        };

        long totalCount = await context.ProductListingViews
            .CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        List<ProductViewsDto> items = await context.ProductListingViews
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .Project(Projection)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductViewsDto>
        {
            Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount
        };
    }
}
