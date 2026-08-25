namespace ProductCatalogService.Application.Features.Queries.GetCategoryAttributes;

public record GetCategoryAttributesQuery(string CategoryId) : IRequest<List<CategoryAttributeDto>>;

public class GetCategoryAttributes(IApplicationDbContext context, ICacheService cache)
    : IRequestHandler<GetCategoryAttributesQuery, List<CategoryAttributeDto>>
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public async Task<List<CategoryAttributeDto>> Handle(GetCategoryAttributesQuery request,
        CancellationToken cancellationToken)
    {
        string cacheKey = $"category-attributes:{request.CategoryId}";

        List<CategoryAttributeDto>? cached =
            await cache.GetAsync<List<CategoryAttributeDto>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        List<CategoryAttribute> attributes = await context.CategoryAttributes
            .Find(a => a.CategoryId == request.CategoryId)
            .SortBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);

        List<CategoryAttributeDto> result =
        [
            .. attributes.Select(a => new CategoryAttributeDto
            {
                Id = a.Id,
                CategoryId = a.CategoryId,
                Name = a.Name,
                Required = a.Required,
                InputType = a.InputType,
                Options = a.Options,
                CompletionWeight = a.CompletionWeight,
                SortOrder = a.SortOrder
            })
        ];

        await cache.SetAsync(cacheKey, result, CacheDuration, cancellationToken);

        return result;
    }
}
