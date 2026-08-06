namespace ReviewService.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    IMongoCollection<Review> Reviews { get; }

    IMongoCollection<ReviewableOrderItem> ReviewableOrderItems { get; }

    IMongoCollection<ReviewAggregate> ReviewAggregates { get; }
}
