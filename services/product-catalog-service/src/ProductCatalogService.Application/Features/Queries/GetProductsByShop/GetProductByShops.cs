namespace ProductCatalogService.Application.Features.Queries.GetProductsByShop;

public record GetProductsByShopQuery(string ShopId, string? Status, int Page, int PageSize)
    : IRequest<PagedResult<ProductDto>>;

public class GetProductsByShop(IApplicationDbContext context)
    : IRequestHandler<GetProductsByShopQuery, PagedResult<ProductDto>>
{
    public async Task<PagedResult<ProductDto>> Handle(GetProductsByShopQuery request,
        CancellationToken cancellationToken)
    {
        FilterDefinitionBuilder<Product> fb = Builders<Product>.Filter;
        FilterDefinition<Product> filter = fb.Eq(p => p.ShopId, request.ShopId);

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse(request.Status, true, out ProductStatus status))
        {
            filter &= fb.Eq(p => p.Status, status);
        }

        long totalCount = await context.Products.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        List<Product> items = await context.Products
            .Find(filter)
            .SortByDescending(p => p.UpdatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Limit(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductDto>
        {
            Items = [.. items.Select(ProductMapper.ToDto)],
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }
}
