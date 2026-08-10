namespace ReviewService.Application.Common.Dtos;

public record ReviewAggregateDto(
    string ProductId,
    double RatingAverage,
    long RatingCount,
    Dictionary<string, int> StarCounts,
    long CommentCount,
    long MediaCount,
    DateTimeOffset UpdatedAt);
