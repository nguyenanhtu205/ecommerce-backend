namespace Common.Contracts.Events;

public record ReviewAggregateUpdated(
    string ProductId,
    double RatingAverage,
    long RatingCount,
    DateTimeOffset UpdatedAt);


public record ReviewMediaAttached(
    string ReviewId,
    string BuyerId,
    List<MediaAttachmentItem> MediaAttachments,
    DateTimeOffset OccurredAt);
