namespace SellerService.Application.Features.Commands.CreateShop;

public class Validator : AbstractValidator<CreateShopCommand>
{
    public Validator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Shop name is required.")
            .MaximumLength(255).WithMessage("Shop name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.")
            .MaximumLength(255).WithMessage("Email must not exceed 256 characters.");

        RuleFor(x => x.PickupAddressId)
            .NotEmpty().WithMessage("Pickup address is required.");
    }
}
