namespace ReviewService.Domain.Entities;

public class ReviewAggregate
{
    public required string Id { get; init; }

    public required double RatingAverage { get; init; }

    public required int RatingCount { get; init; }

    public List<StarCount> StarCounts { get; init; } = [];

    public required int CommentCount { get; init; }

    public required int MediaCount { get; init; }

    public required DateTimeOffset UpdatedAt { get; init; }
}

public class StarCount
{
    public required string Star { get; init; }

    public required int Count { get; init; }
}
