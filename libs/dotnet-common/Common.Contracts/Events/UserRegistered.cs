namespace Common.Contracts.Events;

public record UserRegistered(Guid UserId, string Email, DateTimeOffset RegisteredAt);
