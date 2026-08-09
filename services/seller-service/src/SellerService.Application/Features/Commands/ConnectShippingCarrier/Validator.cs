namespace SellerService.Application.Features.Commands.ConnectShippingCarrier;

public class Validator : AbstractValidator<ConnectShippingCarrierCommand>
{
    public Validator()
    {
        RuleFor(x => x.ShopId)
            .NotEmpty().WithMessage("Shop id is required.");

        RuleFor(x => x.CarrierId)
            .NotEmpty().WithMessage("Carrier id is required.");
    }
}
