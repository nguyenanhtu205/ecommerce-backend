namespace ProductCatalogService.Application.Features.Queries.GetProductViewById;

public record GetProductViewByIdQuery(string Id) : IRequest<ProductViewDto>;

public class GetProductViewById(IApplicationDbContext context, ICacheService cache)
    : IRequestHandler<GetProductViewByIdQuery, ProductViewDto>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);

    public async Task<ProductViewDto> Handle(GetProductViewByIdQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"product-view:{request.Id}";

        ProductViewDto? cached = await cache.GetAsync<ProductViewDto>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        ProductListingView? productListingView = await context.ProductListingViews
            .Find(p => p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (productListingView is null)
        {
            throw new NotFoundException("Product not found.");
        }

        Product? product = await context.Products
            .Find(p => p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
        {
            throw new NotFoundException("Product not found.");
        }

        ProductViewDto dto = ProductViewMapper.ToDto(productListingView, product);

        await cache.SetAsync(cacheKey, dto, CacheDuration, cancellationToken);

        return dto;
    }
}
