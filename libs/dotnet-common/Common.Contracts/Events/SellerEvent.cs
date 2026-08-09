namespace Common.Contracts.Events;

public record ShopCreated(Guid SellerId, Guid ShopId, string ShopName, DateTimeOffset CreatedAt);

public record ShopActivated(string Email, string Purpose, DateTimeOffset ActivatedAt);
