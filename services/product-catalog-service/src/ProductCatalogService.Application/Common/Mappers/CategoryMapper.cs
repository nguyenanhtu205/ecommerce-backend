namespace ProductCatalogService.Application.Common.Mappers;

public static class CategoryMapper
{
    public static CategorySideBarItemDto ToDto(Category category)
    {
        return new CategorySideBarItemDto
        {
            Id = category.Id, Name = category.Name, Slug = category.Slug, IsLeaf = category.IsLeaf
        };
    }
}
