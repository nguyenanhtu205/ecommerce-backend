namespace ProductCatalogService.Application.Features.Queries.GetCategorySidebar;

public record GetCategorySidebarQuery(string Slug) : IRequest<CategorySidebarDto>;

public class GetCategorySidebar(IApplicationDbContext context)
    : IRequestHandler<GetCategorySidebarQuery, CategorySidebarDto>
{
    public async Task<CategorySidebarDto> Handle(GetCategorySidebarQuery request,
        CancellationToken cancellationToken)
    {
        Category? current = await context.Categories
            .Find(c => c.Slug == request.Slug)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            throw new NotFoundException("Category not found.");
        }

        List<Category> children = await context.Categories
            .Find(c => c.ParentId == current.Id)
            .SortBy(c => c.Name)
            .ToListAsync(cancellationToken);

        if (children.Count > 0)
        {
            return new CategorySidebarDto
            {
                Parent = CategoryMapper.ToDto(current),
                Items = [.. children.Select(CategoryMapper.ToDto)],
                ActiveSlug = current.Slug
            };
        }

        Category? parent = current.ParentId is null
            ? null
            : await context.Categories
                .Find(c => c.Id == current.ParentId)
                .FirstOrDefaultAsync(cancellationToken);

        List<Category> siblings = await context.Categories
            .Find(c => c.ParentId == current.ParentId)
            .SortBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return new CategorySidebarDto
        {
            Parent = parent is null ? null : CategoryMapper.ToDto(parent),
            Items = [.. siblings.Select(CategoryMapper.ToDto)],
            ActiveSlug = current.Slug
        };
    }
}
