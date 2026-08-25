namespace ProductCatalogService.Application.Common.Dtos;

public class CategorySidebarDto
{
    public CategorySideBarItemDto? Parent { get; init; }

    public List<CategorySideBarItemDto> Items { get; init; } = [];

    public required string ActiveSlug { get; init; }
}

public class CategorySideBarItemDto
{
    public required string Id { get; init; }

    public required string Name { get; init; }

    public required string Slug { get; init; }

    public required bool IsLeaf { get; init; }
}
