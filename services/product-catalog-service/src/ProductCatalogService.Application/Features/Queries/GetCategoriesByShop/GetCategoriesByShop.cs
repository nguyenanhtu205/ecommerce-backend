namespace ProductCatalogService.Application.Features.Queries.GetCategoriesByShop;

public record CategoryByShop(string Id, string Name);

public record GetCategoriesByShopQuery(string ShopId) : IRequest<List<CategoryByShop>>;

public class GetCategoriesByShop(IApplicationDbContext context)
    : IRequestHandler<GetCategoriesByShopQuery, List<CategoryByShop>>
{
    public async Task<List<CategoryByShop>> Handle(
        GetCategoriesByShopQuery request,
        CancellationToken cancellationToken)
    {
        List<CategoryByShop> result = await context.ProductListingViews
            .Aggregate()
            .Match(x => x.ShopId == request.ShopId)
            .Unwind(x => x.CategoryPath)
            .Group(new BsonDocument
            {
                { "_id", "$categoryPath._id" }, 
                { "name", new BsonDocument("$first", "$categoryPath.name") }
            })
            .As<CategoryByShop>()
            .ToListAsync(cancellationToken);

        return result;
    }
}
