namespace SellerService.Application.Features.Queries.GetShopInformation;

public record GetShopVacationSettingResponse(bool IsEnabled, DateOnly? StartDate, DateOnly? EndDate, string? Message);

public record GetShopVacationSettingQuery : IRequest<GetShopVacationSettingResponse>;

public class GetShopVacationSetting(IApplicationDbContext context, ICurrentUser currentUser)
    : IRequestHandler<GetShopVacationSettingQuery, GetShopVacationSettingResponse>
{
    public async Task<GetShopVacationSettingResponse> Handle(GetShopVacationSettingQuery request,
        CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        ShopVacationSetting? vacationSetting = await context.ShopVacationSettings
            .AsNoTracking()
            .Where(s => s.ShopId == currentUser.ShopId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        return vacationSetting is null
            ? new GetShopVacationSettingResponse(false, null, null, null)
            : new GetShopVacationSettingResponse(vacationSetting.IsEnabled, vacationSetting.StartDate,
                vacationSetting.EndDate, vacationSetting.Message);
    }
}
