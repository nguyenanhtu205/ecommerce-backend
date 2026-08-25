namespace SellerService.Application.Features.Commands.UpdateShopChatSetting;

public class Validator : AbstractValidator<UpdateShopChatSettingCommand>
{
    public Validator()
    {
        RuleFor(x => x.AutoReplyEnabled)
            .NotNull().WithMessage("Auto reply enabled is required.");

        RuleFor(x => x.AutoReplyMessage)
            .NotNull().WithMessage("Message is required.");
    }
}
