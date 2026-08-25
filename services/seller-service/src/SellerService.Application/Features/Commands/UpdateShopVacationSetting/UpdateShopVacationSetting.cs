namespace SellerService.Application.Features.Commands.UpdateShopVacationSetting;

public record UpdateShopVacationSettingCommand(bool IsEnabled, DateOnly? StartDate, DateOnly? EndDate, string? Message)
    : IRequest;

public class UpdateShopVacationSetting(
    IApplicationDbContext context,
    ICurrentUser currentUser,
    ITopicProducer<ShopVacationSettingUpdated> producer)
    : IRequestHandler<UpdateShopVacationSettingCommand>
{
    public async Task Handle(UpdateShopVacationSettingCommand request, CancellationToken cancellationToken)
    {
        if (currentUser.ShopId is null)
        {
            throw new UnauthorizedAccessException();
        }

        ShopVacationSetting? vacationSetting = await context.ShopVacationSettings
            .Where(s => s.ShopId == currentUser.ShopId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (vacationSetting is null)
        {
            ShopVacationSetting newVacationSetting = new()
            {
                ShopId = currentUser.ShopId.Value,
                IsEnabled = request.IsEnabled,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Message = request.Message
            };

            context.ShopVacationSettings.Add(newVacationSetting);
        }
        else
        {
            vacationSetting.IsEnabled = request.IsEnabled;
            vacationSetting.StartDate = request.StartDate;
            vacationSetting.EndDate = request.EndDate;
            vacationSetting.Message = request.Message;
        }

        await context.SaveChangesAsync(cancellationToken);

        if (request.IsEnabled)
        {
            await producer.Produce(new ShopVacationSettingUpdated(currentUser.ShopId.Value.ToString(), true,
                    request.StartDate!.Value, request.EndDate!.Value, request.Message!),
                cancellationToken);
        }
    }
}
