namespace SellerService.Application.Features.Queries.GetShopInformation;

public record ShopVacation(bool IsEnabled, DateOnly StartDate, DateOnly EndDate, string Message);

public record GetShopInformationForBuyerResponse(
    string Name,
    ShopVacation? ShopVacation,
    DateTimeOffset CreatedAt,
    string Location,
    List<string> CarrierCodes,
    string? Description,
    string? ShopAvatarUrl);

public record GetShopInformationForBuyerQuery(string ShopId) : IRequest<GetShopInformationForBuyerResponse>;

public class GetShopInformationForBuyer(IApplicationDbContext context)
    : IRequestHandler<GetShopInformationForBuyerQuery, GetShopInformationForBuyerResponse>
{
    public async Task<GetShopInformationForBuyerResponse> Handle(
        GetShopInformationForBuyerQuery request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.ShopId, out Guid shopId))
        {
            throw new NotFoundException("Shop not found.");
        }

        GetShopInformationForBuyerResponse? response = await context.Shops
            .AsNoTracking()
            .Where(s => s.Id == shopId && s.Status == ShopStatus.Active)
            .Select(s => new GetShopInformationForBuyerResponse(
                s.Name,
                s.ShopVacationSetting != null
                && s.ShopVacationSetting.IsEnabled
                && s.ShopVacationSetting.StartDate.HasValue
                && s.ShopVacationSetting.EndDate.HasValue
                    ? new ShopVacation(
                        s.ShopVacationSetting.IsEnabled,
                        s.ShopVacationSetting.StartDate!.Value,
                        s.ShopVacationSetting.EndDate!.Value,
                        s.ShopVacationSetting.Message ?? string.Empty)
                    : null,
                s.CreatedAt,
                $"{s.PickupAddressSnapshot.Ward}, {s.PickupAddressSnapshot.Province}",
                s.ShippingCarrierConnections
                    .Where(c => c.Status == ShopShippingCarrierConnectionStatus.Connected)
                    .Select(c => c.CarrierCode)
                    .ToList(),
                s.Description,
                s.ShopAvatarUrl
            ))
            .FirstOrDefaultAsync(cancellationToken);

        return response ?? throw new NotFoundException("Shop not found");
    }
}
