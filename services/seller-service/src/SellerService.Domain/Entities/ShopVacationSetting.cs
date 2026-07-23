namespace SellerService.Domain.Entities;

public class ShopVacationSetting
{
    public required Guid ShopId { get; init; }

    public required bool IsEnabled { get; init; }

    public DateOnly? StartDate { get; init; }

    public DateOnly? EndDate { get; init; }

    public string? Message { get; init; }

    public Shop? Shop { get; init; }
}
