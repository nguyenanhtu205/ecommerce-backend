namespace SellerService.Application.Features.Commands.ActivateShop;

public class Validator : AbstractValidator<ActivateShopCommand>
{
    public Validator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("Shop id is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(255).WithMessage("Email must not exceed 256 characters.");
    }
}
