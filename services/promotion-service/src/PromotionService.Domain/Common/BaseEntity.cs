namespace PromotionService.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; } = Guid.CreateVersion7();
}
