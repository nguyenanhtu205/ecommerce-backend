namespace ProductCatalogService.Domain.Entities;

public class CategoryAttribute
{
    public required string Id { get; init; }

    public required string CategoryId { get; init; }

    public required string Name { get; init; }

    public bool Required { get; init; }

    public required string InputType { get; init; }

    public List<string> Options { get; init; } = [];

    public int CompletionWeight { get; init; }

    public int SortOrder { get; init; }
}
