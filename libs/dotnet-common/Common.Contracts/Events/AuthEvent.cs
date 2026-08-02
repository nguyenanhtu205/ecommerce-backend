namespace Common.Contracts.Events;

public record OtpRequested(string Email, string Code, string Purpose, DateTimeOffset RequestedAt);

public record UserRegistered(Guid UserId, string Email, DateTimeOffset RegisteredAt);
