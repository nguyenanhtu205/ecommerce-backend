namespace ProductCatalogService.Application.Features.Queries.GetCategories;

public record GetCategoriesQuery(string? ParentId) : IRequest<List<CategoryDto>>;

public class GetCategories(IApplicationDbContext context, ICacheService cache)
    : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        string cacheKey = $"categories:{request.ParentId ?? "root"}";

        List<CategoryDto>? cached = await cache.GetAsync<List<CategoryDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        FilterDefinition<Category> filter = string.IsNullOrEmpty(request.ParentId)
            ? Builders<Category>.Filter.Eq(c => c.ParentId, null)
            : Builders<Category>.Filter.Eq(c => c.ParentId, request.ParentId);

        List<Category> categories = await context.Categories
            .Find(filter)
            .SortBy(c => c.Name)
            .ToListAsync(cancellationToken);

        List<CategoryDto> result =
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

        await cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);

        return result;
    }
}
