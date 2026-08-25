namespace ProductCatalogService.Application.Features.Queries.GetSimilarProducts;

public record GetSimilarProductsQuery(string ProductId) : IRequest<List<ProductViewsDto>>;

public class GetSimilarProducts(IApplicationDbContext context)
    : IRequestHandler<GetSimilarProductsQuery, List<ProductViewsDto>>
{
    private const int MaxResults = 5;

    public async Task<List<ProductViewsDto>> Handle(GetSimilarProductsQuery request,
        CancellationToken cancellationToken)
    {
        ProductListingView source = await context.ProductListingViews
                                        .Find(v => v.Id == request.ProductId)
                                        .FirstOrDefaultAsync(cancellationToken)
                                    ?? throw new NotFoundException("Product not found.");

        if (source.CategoryPath.Count == 0)
        {
            return [];
        }

        List<ProductListingView> results = [];
        HashSet<string> excludedIds = [request.ProductId];

        for (int i = source.CategoryPath.Count - 1; i >= 0 && results.Count < MaxResults; i--)
        {
            string categoryId = source.CategoryPath[i].Id;
            int remaining = MaxResults - results.Count;

            FilterDefinition<ProductListingView> filter = Builders<ProductListingView>.Filter.And(
                Builders<ProductListingView>.Filter.ElemMatch(v => v.CategoryPath, p => p.Id == categoryId),
                Builders<ProductListingView>.Filter.Nin(v => v.Id, excludedIds));

            List<ProductListingView> matches = await context.ProductListingViews
                .Find(filter)
                .SortByDescending(v => v.SoldCount)
                .ThenByDescending(v => v.RatingAverage)
                .Limit(remaining)
                .ToListAsync(cancellationToken);

            results.AddRange(matches);
            foreach (ProductListingView match in matches)
            {
                excludedIds.Add(match.Id);
            }
        }

        return
        [
            .. results.Select(v => new ProductViewsDto
            {
                Id = v.Id,
                Name = v.Name,
                ThumbnailUrl = v.ThumbnailUrl,
                PriceMin = v.PriceMin,
                PriceMax = v.PriceMax,
                OriginalPriceMin = v.OriginalPriceMin,
                DiscountPercent = v.DiscountPercent,
                IsOutOfStock = v.IsOutOfStock,
                StockTotal = v.StockTotal,
                RatingAverage = v.RatingAverage,
                RatingCount = v.RatingCount,
                SoldCount = v.SoldCount,
                Location = v.Location
            })
        ];
    }
}
