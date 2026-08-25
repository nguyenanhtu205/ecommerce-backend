namespace Common.Outbox;

public class ProcessedEvent
{
    public required Guid Id { get; set; }

    public required string EventId { get; set; }

    public required string EventType { get; set; }

    public required DateTimeOffset ProcessedAt { get; set; }
}
