namespace ProductCatalogService.Application.Features.Queries.GetCategories;

public record GetCategoriesQuery(string? ParentId) : IRequest<List<CategoryDto>>;

public class GetCategories(IApplicationDbContext context) : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        FilterDefinition<Category> filter = string.IsNullOrEmpty(request.ParentId)
            ? Builders<Category>.Filter.Eq(c => c.ParentId, null)
            : Builders<Category>.Filter.Eq(c => c.ParentId, request.ParentId);

        List<Category> categories = await context.Categories
            .Find(filter)
            .SortBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return
        [
            .. categories.Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                ParentId = c.ParentId,
                Path = [.. c.Path.Select(p => new CategoryPathItemDto { Id = p.Id, Slug = p.Slug, Name = p.Name })],
                Level = c.Level,
                IsLeaf = c.IsLeaf
            })
        ];
    }
}
