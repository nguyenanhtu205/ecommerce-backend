namespace ProductCatalogService.Application.Features.Queries.GetProductById;

public record GetProductByIdQuery(string Id) : IRequest<ProductDto>;

public class GetProductById(IApplicationDbContext context) : IRequestHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<ProductDto> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Find(p => p.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return product is null ? throw new NotFoundException("Product not found.") : ProductMapper.ToDto(product);
    }
}
