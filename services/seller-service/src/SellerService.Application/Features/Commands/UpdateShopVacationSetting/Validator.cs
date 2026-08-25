namespace SellerService.Application.Features.Commands.UpdateShopVacationSetting;

public class Validator : AbstractValidator<UpdateShopVacationSettingCommand>
{
    public Validator()
    {
        RuleFor(x => x.IsEnabled)
            .NotNull().WithMessage("Is enabled is required.");
    }
}
