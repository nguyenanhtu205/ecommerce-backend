using Common.Application.Interfaces;

namespace ProductCatalogService.Application.Features.Queries.GetProductsForSeller;

public record GetProductsForSellerResponse(
    string Id,
    string Name,
    string Description,
    List<string> Tags,
    ProductStatus Status,
    string ThumbnailMediaId,
    string? VideoMediaId,
    List<string> GalleryMediaIds,
    List<Specification> Specifications,
    List<VariantGroup> VariantGroups,
    List<VariantCombination> VariantCombinations,
    ShippingInfo ShippingInfo
);

public record GetProductsForSellerQuery : IRequest<List<GetProductsForSellerResponse>>;

public class GetProductsForSeller(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetProductsForSellerQuery, List<GetProductsForSellerResponse>>
{
    public async Task<List<GetProductsForSellerResponse>> Handle(GetProductsForSellerQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        string shopId = currentUser.ShopId.Value.ToString();

        List<Product> products = await context.Products
            .Find(p => p.ShopId == shopId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return
        [
            .. products
                .Select(p => new GetProductsForSellerResponse(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.Tags,
                    p.Status,
                    p.ThumbnailMediaId,
                    p.VideoMediaId,
                    p.GalleryMediaIds,
                    p.Specifications,
                    p.VariantGroups,
                    p.VariantCombinations,
                    p.ShippingInfo))
        ];
    }
}
