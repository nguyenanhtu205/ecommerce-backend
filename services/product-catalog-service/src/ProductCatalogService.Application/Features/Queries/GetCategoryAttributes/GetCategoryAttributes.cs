namespace ProductCatalogService.Application.Features.Queries.GetCategoryAttributes;

public record GetCategoryAttributesQuery(string CategoryId) : IRequest<List<CategoryAttributeDto>>;

public class GetCategoryAttributes(IApplicationDbContext context)
    : IRequestHandler<GetCategoryAttributesQuery, List<CategoryAttributeDto>>
{
    public async Task<List<CategoryAttributeDto>> Handle(GetCategoryAttributesQuery request,
        CancellationToken cancellationToken)
    {
        List<CategoryAttribute> attributes = await context.CategoryAttributes
            .Find(a => a.CategoryId == request.CategoryId)
            .SortBy(a => a.SortOrder)
            .ToListAsync(cancellationToken);

        return
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
    }
}
