using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ProductCatalogService.Application.Features.Queries.GetProductViewsByCondition;

public enum ProductSortBy
{
    Newest = 0,
    PriceAsc = 1,
    PriceDesc = 2,
    RatingAsc = 3,
    RatingDesc = 4,
    BestSelling = 5,
    RatingCountDesc = 6
}

public record GetProductViewsByConditionQuery(
    string? CategorySlug,
    string? Province,
    ProductSortBy? SortBy,
    int Page,
    int PageSize,
    double? RatingFrom,
    double? RatingTo,
    decimal? PriceMin,
    decimal? PriceMax,
    bool? InStockOnly,
    bool? LowStockOnly
) : IRequest<PagedResult<ProductViewsDto>>;

public class GetProductViewsByCondition(IApplicationDbContext context)
    : IRequestHandler<GetProductViewsByConditionQuery, PagedResult<ProductViewsDto>>
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 20;
    private const int LowStockThreshold = 10;

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

    public async Task<PagedResult<ProductViewsDto>> Handle(GetProductViewsByConditionQuery request,
        CancellationToken cancellationToken)
    {
        FilterDefinitionBuilder<ProductListingView> fb = Builders<ProductListingView>.Filter;
        List<FilterDefinition<ProductListingView>> filters = [];

        if (!string.IsNullOrWhiteSpace(request.CategorySlug))
        {
            filters.Add(fb.ElemMatch(p => p.CategoryPath, cp => cp.Slug == request.CategorySlug));
        }

        if (!string.IsNullOrWhiteSpace(request.Province))
        {
            string escaped = Regex.Escape(request.Province.Trim());
            filters.Add(fb.Regex(p => p.Location, new BsonRegularExpression(escaped, "i")));
        }

        if (request.RatingFrom is > 0)
        {
            filters.Add(fb.Gte(p => p.RatingAverage, request.RatingFrom.Value));
        }

        if (request.RatingTo is > 0)
        {
            filters.Add(fb.Lte(p => p.RatingAverage, request.RatingTo.Value));
        }

        if (request.PriceMin is > 0)
        {
            filters.Add(fb.Gte(p => p.PriceMax, request.PriceMin.Value));
        }

        if (request.PriceMax is > 0)
        {
            filters.Add(fb.Lte(p => p.PriceMin, request.PriceMax.Value));
        }

        if (request.InStockOnly == true)
        {
            filters.Add(fb.Eq(p => p.IsOutOfStock, false));
        }

        if (request.LowStockOnly == true)
        {
            filters.Add(fb.Lt(p => p.StockTotal, LowStockThreshold));
        }

        FilterDefinition<ProductListingView> filter = filters.Count > 0
            ? fb.And(filters)
            : fb.Empty;

        int page = request.Page > 0 ? request.Page : DefaultPage;
        int pageSize = request.PageSize switch
        {
            > 0 and <= MaxPageSize => request.PageSize,
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
            ProductSortBy.RatingCountDesc => Builders<ProductListingView>.Sort.Descending(p => p.RatingCount),
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
