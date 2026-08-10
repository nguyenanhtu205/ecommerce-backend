namespace ProductCatalogService.Application.Features.Queries.GetProductViewById;

public record GetProductViewByIdQuery(string Id) : IRequest<ProductViewDto>;

public class GetProductViewById(IApplicationDbContext context)
    : IRequestHandler<GetProductViewByIdQuery, ProductViewDto>
{
    public async Task<ProductViewDto> Handle(GetProductViewByIdQuery request, CancellationToken cancellationToken)
    {
        ProductListingView? productListingView = await context.ProductListingViews
            .Find(p => p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return productListingView is null
            ? throw new NotFoundException("Product not found.")
            : ProductViewMapper.ToDto(productListingView);
    }
}
