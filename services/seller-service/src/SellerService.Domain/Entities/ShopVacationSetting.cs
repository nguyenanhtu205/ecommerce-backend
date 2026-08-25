namespace SellerService.Domain.Entities;

public class ShopVacationSetting
{
    public required Guid ShopId { get; init; }

    public required bool IsEnabled { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Message { get; set; }

    public Shop? Shop { get; init; }
}
