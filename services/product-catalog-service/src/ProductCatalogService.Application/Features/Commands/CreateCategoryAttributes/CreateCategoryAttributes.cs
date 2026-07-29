namespace ProductCatalogService.Application.Features.Commands.CreateCategoryAttributes;

public record CreateCategoryAttributesCommand(
    string CategoryId,
    string Name,
    bool Required,
    string InputType,
    List<string> Options,
    int CompletionWeight,
    int SortOrder
) : IRequest;

public class CreateCategoryAttributes(IApplicationDbContext context) : IRequestHandler<CreateCategoryAttributesCommand>
{
    public async Task Handle(CreateCategoryAttributesCommand request, CancellationToken cancellationToken)
    {
        CategoryAttribute categoryAttribute = new()
        {
            Id = Guid.CreateVersion7().ToString(),
            CategoryId = request.CategoryId,
            Name = request.Name,
            Required = request.Required,
            InputType = request.InputType,
            Options = request.Options,
            CompletionWeight = request.CompletionWeight,
            SortOrder = request.SortOrder
        };

        await context.CategoryAttributes.InsertOneAsync(categoryAttribute, cancellationToken: cancellationToken);
    }
}
