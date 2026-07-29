namespace ProductCatalogService.Application.Features.Commands.CreateCategory;

public record CreateCategoryResponse(string CategoryId);

public record CreateCategoryCommand(
    string Name,
    string Slug,
    string? ParentId,
    List<CategoryPathItem> Path,
    int Level,
    bool IsLeaf) : IRequest<CreateCategoryResponse>;

public class CreateCategory(IApplicationDbContext context)
    : IRequestHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    public async Task<CreateCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        Category category = new()
        {
            Id = Guid.CreateVersion7().ToString(),
            Name = request.Name,
            Slug = request.Slug,
            ParentId = request.ParentId,
            Path = request.Path,
            Level = request.Level,
            IsLeaf = request.IsLeaf,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await context.Categories.InsertOneAsync(category, cancellationToken: cancellationToken);

        return new CreateCategoryResponse(category.Id);
    }
}
