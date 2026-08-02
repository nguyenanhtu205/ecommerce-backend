namespace Common.Contracts.Events;

public record ShopActivated(string Email, string Purpose, DateTimeOffset ActivatedAt);
