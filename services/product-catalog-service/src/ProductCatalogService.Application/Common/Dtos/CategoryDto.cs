namespace ProductCatalogService.Application.Common.Dtos;

public class CategoryDto
{
    public required string Id { get; set; }

    public required string Name { get; set; }

    public required string Slug { get; set; }

    public string? ParentId { get; set; }

    public List<CategoryPathItemDto> Path { get; set; } = [];

    public int Level { get; set; }

    public bool IsLeaf { get; set; }
}

public class CategoryAttributeDto
{
    public required string Id { get; set; }

    public required string CategoryId { get; set; }

    public required string Name { get; set; }

    public bool Required { get; set; }

    public required string InputType { get; set; }

    public List<string> Options { get; set; } = [];

    public int CompletionWeight { get; set; }

    public int SortOrder { get; set; }
}
