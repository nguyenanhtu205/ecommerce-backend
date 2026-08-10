namespace ProductCatalogService.Domain.Entities;

public class Category
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Slug { get; init; }

    public string? ParentId { get; init; }

    public List<CategoryPathItem> Path { get; init; } = [];

    public int Level { get; init; }

    public bool IsLeaf { get; init; }

    public DateTimeOffset CreatedAt { get; init; }
}

public class CategoryPathItem
{
    public required string Id { get; init; }

    public required string Slug { get; init; }

    public required string Name { get; init; }
}
