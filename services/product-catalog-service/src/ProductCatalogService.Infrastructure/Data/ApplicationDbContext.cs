namespace ProductCatalogService.Infrastructure.Data;

public class ApplicationDbContext(IMongoDatabase database) : IApplicationDbContext
{
    public IMongoCollection<Category> Categories => database.GetCollection<Category>("categories");

    public IMongoCollection<CategoryAttribute> CategoryAttributes =>
        database.GetCollection<CategoryAttribute>("category_attributes");

    public IMongoCollection<Product> Products => database.GetCollection<Product>("products");

    public IMongoCollection<ProductListingView> ProductListingViews =>
        database.GetCollection<ProductListingView>("product_listing_view");
}
