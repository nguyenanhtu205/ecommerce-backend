namespace ReviewService.Domain.Entities;

public class ReviewAggregate
{
    public required string Id { get; init; }

    public required double RatingAverage { get; init; }

    public required int RatingSum { get; set; }

    public required int RatingCount { get; init; }

    public Dictionary<string, int> StarCounts { get; init; } = [];

    public required int CommentCount { get; init; }

    public required int MediaCount { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}
