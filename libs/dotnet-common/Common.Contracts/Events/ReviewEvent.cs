namespace Common.Contracts.Events;

public record ReviewAggregateUpdated(
    string ProductId,
    double RatingAverage,
    long RatingCount,
    DateTimeOffset UpdatedAt);
