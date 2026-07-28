namespace InventoryService.Domain.Entities;

public class ProcessedEvent : BaseEntity
{
    public required string EventId { get; init; }

    public required string EventType { get; init; }

    public required DateTimeOffset ProcessedAt { get; init; }
}
