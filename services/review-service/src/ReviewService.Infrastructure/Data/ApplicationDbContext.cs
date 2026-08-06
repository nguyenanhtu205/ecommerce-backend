namespace ReviewService.Infrastructure.Data;

public class ApplicationDbContext(IMongoDatabase database) : IApplicationDbContext
{
    public IMongoCollection<Review> Reviews => database.GetCollection<Review>("reviews");

    public IMongoCollection<ReviewableOrderItem> ReviewableOrderItems =>
        database.GetCollection<ReviewableOrderItem>("reviewable_order_items");

    public IMongoCollection<ReviewAggregate> ReviewAggregates =>
        database.GetCollection<ReviewAggregate>("review_aggregates");
}
