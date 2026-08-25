namespace Common.Contracts.Events;

public record ShopCreated(Guid SellerId, Guid ShopId, string ShopName, DateTimeOffset CreatedAt);

public record ShopActivated(string Email, string Purpose, DateTimeOffset ActivatedAt);

public record ShopNameChanged(string ShopId, string ShopName, DateTimeOffset ChangedAt);

public record ShopChatSettingUpdated(string ShopId, bool AutoReplyEnabled, string AutoReplyMessage);

public record ShopVacationSettingUpdated(
    string ShopId,
    bool IsEnabled,
    DateOnly StartDate,
    DateOnly EndDate,
    string Message);
