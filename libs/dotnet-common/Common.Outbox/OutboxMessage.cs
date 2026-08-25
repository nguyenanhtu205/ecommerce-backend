namespace Common.Outbox;

public class OutboxMessage
{
    public required Guid Id { get; set; }

    public required string MessageType { get; set; }

    public required string Payload { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }

    public int AttemptCount { get; set; }

    public string? Error { get; set; }
}
