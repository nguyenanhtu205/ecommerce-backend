namespace Common.Contracts.Events;

public record UserRegisteredEvent(Guid UserId, string Email, DateTimeOffset RegisteredAt);
