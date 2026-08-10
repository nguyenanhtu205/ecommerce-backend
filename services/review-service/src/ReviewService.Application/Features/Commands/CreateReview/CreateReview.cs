using Attribute = ReviewService.Domain.Entities.Attribute;

namespace ReviewService.Application.Features.Commands.CreateReview;

public record CreateReviewCommand(
    string OrderItemId,
    int Rating,
    string Comment,
    List<ReviewAttributeDto> Attributes,
    List<string> MediaAssetIds) : IRequest<ReviewDto>;

public class CreateReview(
    ICurrentUser currentUser,
    IApplicationDbContext context,
    ITopicProducer<ReviewAggregateUpdated> producer
) : IRequestHandler<CreateReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(CreateReviewCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            throw new UnauthorizedAccessException();
        }

        ReviewableOrderItem orderItem = await context.ReviewableOrderItems
                                            .Find(x => x.Id == request.OrderItemId)
                                            .FirstOrDefaultAsync(cancellationToken) ??
                                        throw new NotFoundException("Order item not found");

        if (orderItem.BuyerId != currentUser.UserId.ToString())
        {
            throw new ForbiddenAccessException();
        }

        if (orderItem.IsReviewed)
        {
            throw new ConflictException("You have already reviewed this product.");
        }

        string reviewId = Guid.CreateVersion7().ToString();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        Review review = new()
        {
            Id = reviewId,
            OrderItemId = request.OrderItemId,
            ProductId = orderItem.ProductId,
            ShopId = orderItem.ShopId,
            BuyerId = orderItem.BuyerId,
            BuyerDisplayName = currentUser.Email,
            Rating = request.Rating,
            Variation = orderItem.Variation ?? "",
            Attributes =
            [
                .. request.Attributes.Select(a => new Attribute { Label = a.Label, Value = a.Value })
            ],
            Comment = request.Comment,
            MediaAssetIds = request.MediaAssetIds,
            LikeCount = 0,
            CreatedAt = now
        };

        await context.Reviews.InsertOneAsync(review, cancellationToken: cancellationToken);

        await context.ReviewableOrderItems.UpdateOneAsync(
            x => x.Id == request.OrderItemId,
            Builders<ReviewableOrderItem>.Update.Set(x => x.IsReviewed, true),
            cancellationToken: cancellationToken);

        DateTimeOffset aggregateUpdatedAt = DateTimeOffset.UtcNow;
        bool hasComment = !string.IsNullOrWhiteSpace(request.Comment);
        bool hasMedia = request.MediaAssetIds.Count > 0;
        string starField = $"starCounts.{request.Rating}";

        UpdateDefinition<ReviewAggregate> update = Builders<ReviewAggregate>.Update.Pipeline(
            new[]
            {
                new BsonDocument("$set", new BsonDocument
                {
                    ["ratingSum"] = new BsonDocument("$add",
                        new BsonArray
                        {
                            new BsonDocument("$ifNull", new BsonArray { "$ratingSum", 0 }), request.Rating
                        }),
                    ["ratingCount"] = new BsonDocument("$add",
                        new BsonArray { new BsonDocument("$ifNull", new BsonArray { "$ratingCount", 0 }), 1 }),
                    [starField] = new BsonDocument("$add",
                        new BsonArray { new BsonDocument("$ifNull", new BsonArray { $"${starField}", 0 }), 1 }),
                    ["commentCount"] = new BsonDocument("$add",
                        new BsonArray
                        {
                            new BsonDocument("$ifNull", new BsonArray { "$commentCount", 0 }), hasComment ? 1 : 0
                        }),
                    ["mediaCount"] = new BsonDocument("$add",
                        new BsonArray
                        {
                            new BsonDocument("$ifNull", new BsonArray { "$mediaCount", 0 }), hasMedia ? 1 : 0
                        }),
                    ["updatedAt"] = new BsonDateTime(aggregateUpdatedAt.UtcDateTime)
                }),
                new BsonDocument("$set",
                    new BsonDocument
                    {
                        ["ratingAverage"] = new BsonDocument("$round",
                            new BsonArray
                            {
                                new BsonDocument("$divide", new BsonArray { "$ratingSum", "$ratingCount" }), 1
                            })
                    })
            });

        ReviewAggregate aggregate = await context.ReviewAggregates.FindOneAndUpdateAsync(
            x => x.Id == orderItem.ProductId,
            update,
            new FindOneAndUpdateOptions<ReviewAggregate> { IsUpsert = true, ReturnDocument = ReturnDocument.After },
            cancellationToken);

        await producer.Produce(new ReviewAggregateUpdated(
                orderItem.ProductId, aggregate.RatingAverage, aggregate.RatingCount, aggregateUpdatedAt),
            cancellationToken);

        return ReviewMapper.ToDto(review);
    }
}
