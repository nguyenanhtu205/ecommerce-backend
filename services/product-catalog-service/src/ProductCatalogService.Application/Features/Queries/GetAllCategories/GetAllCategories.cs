namespace ProductCatalogService.Application.Features.Queries.GetAllCategories;

public record GetAllCategoriesQuery : IRequest<List<CategoryDto>>;

public class GetAllCategories(IApplicationDbContext context, ICacheService cache)
    : IRequestHandler<GetAllCategoriesQuery, List<CategoryDto>>
{
    private const string CacheKey = "categories:all";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public async Task<List<CategoryDto>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
    {
        List<CategoryDto>? cached = await cache.GetAsync<List<CategoryDto>>(CacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        List<Category> categories = await context.Categories
            .Find(_ => true)
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

        await cache.SetAsync(CacheKey, result, CacheDuration, cancellationToken);

        return result;
    }
}
