namespace ReviewService.Application.Features.Commands.DeleteReview;

public record DeleteReviewCommand(string ReviewId) : IRequest;

public class DeleteReview(
    IApplicationDbContext context,
    ITopicProducer<ReviewAggregateUpdated> producer,
    ICurrentUser currentUser
) : IRequestHandler<DeleteReviewCommand>
{
    public async Task Handle(DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        Review review = await context.Reviews
                            .Find(r => r.Id == request.ReviewId && r.BuyerId == currentUser.UserId.Value.ToString())
                            .FirstOrDefaultAsync(cancellationToken)
                        ?? throw new NotFoundException($"Review '{request.ReviewId}' does not exist.");

        await context.Reviews.DeleteOneAsync(r => r.Id == request.ReviewId, cancellationToken);

        await context.ReviewableOrderItems.UpdateOneAsync(
            x => x.Id == review.OrderItemId,
            Builders<ReviewableOrderItem>.Update.Set(x => x.IsReviewed, false),
            cancellationToken: cancellationToken);

        DateTimeOffset aggregateUpdatedAt = DateTimeOffset.UtcNow;
        bool hasComment = !string.IsNullOrWhiteSpace(review.Comment);
        bool hasMedia = review.MediaAssetIds.Count > 0;
        string starField = $"starCounts.{review.Rating}";

        UpdateDefinition<ReviewAggregate> update = Builders<ReviewAggregate>.Update.Pipeline(
            new[]
            {
                new BsonDocument("$set", new BsonDocument
                {
                    ["ratingSum"] = new BsonDocument("$max",
                        new BsonArray
                        {
                            0,
                            new BsonDocument("$subtract",
                                new BsonArray
                                {
                                    new BsonDocument("$ifNull", new BsonArray { "$ratingSum", 0 }), review.Rating
                                })
                        }),
                    ["ratingCount"] = new BsonDocument("$max",
                        new BsonArray
                        {
                            0,
                            new BsonDocument("$subtract",
                                new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$ratingCount", 0 }), 1 })
                        }),
                    [starField] = new BsonDocument("$max",
                        new BsonArray
                        {
                            0,
                            new BsonDocument("$subtract",
                                new BsonArray { new BsonDocument("$ifNull", new BsonArray { $"${starField}", 0 }), 1 })
                        }),
                    ["commentCount"] = new BsonDocument("$max",
                        new BsonArray
                        {
                            0,
                            new BsonDocument("$subtract",
                                new BsonArray
                                {
                                    new BsonDocument("$ifNull", new BsonArray { "$commentCount", 0 }),
                                    hasComment ? 1 : 0
                                })
                        }),
                    ["mediaCount"] = new BsonDocument("$max",
                        new BsonArray
                        {
                            0,
                            new BsonDocument("$subtract",
                                new BsonArray
                                {
                                    new BsonDocument("$ifNull", new BsonArray { "$mediaCount", 0 }),
                                    hasMedia ? 1 : 0
                                })
                        }),
                    ["updatedAt"] = new BsonDateTime(aggregateUpdatedAt.UtcDateTime)
                }),
                new BsonDocument("$set",
                    new BsonDocument
                    {
                        ["ratingAverage"] = new BsonDocument("$cond", new BsonArray
                        {
                            new BsonDocument("$eq", new BsonArray { "$ratingCount", 0 }),
                            0,
                            new BsonDocument("$round",
                                new BsonArray
                                {
                                    new BsonDocument("$divide", new BsonArray { "$ratingSum", "$ratingCount" }), 1
                                })
                        })
                    })
            });

        ReviewAggregate aggregate = await context.ReviewAggregates.FindOneAndUpdateAsync(
            x => x.Id == review.ProductId,
            update,
            new FindOneAndUpdateOptions<ReviewAggregate> { ReturnDocument = ReturnDocument.After },
            cancellationToken);

        if (aggregate is not null)
        {
            await producer.Produce(new ReviewAggregateUpdated(
                    review.ProductId, aggregate.RatingAverage, aggregate.RatingCount, aggregateUpdatedAt),
                cancellationToken);
        }
    }
}
