namespace ProductCatalogService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IMongoCollection<Category> Categories { get; }

    IMongoCollection<CategoryAttribute> CategoryAttributes { get; }

    IMongoCollection<Product> Products { get; }

    IMongoCollection<ProductListingView> ProductListingViews { get; }
}
