namespace OrderService.Application.Features.Commands.Checkout;

public class Validator : AbstractValidator<CheckoutCommand>
{
    public Validator()
    {
        RuleFor(x => x.CartItems)
            .NotEmpty().WithMessage("Cart items must not be empty.");

        RuleFor(x => x.CartItems)
            .Must(items => items.Select(i => i.CombinationId).Distinct().Count() == items.Count)
            .When(x => x.CartItems is { Count: > 0 })
            .WithMessage("Cart items contain duplicate combinations.");

        RuleForEach(x => x.CartItems).ChildRules(item =>
        {
            item.RuleFor(i => i.CombinationId)
                .NotEmpty().WithMessage("CombinationId is invalid.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        });

        RuleFor(x => x.ShopInfos)
            .NotEmpty().WithMessage("Shop info must not be empty.");

        RuleForEach(x => x.ShopInfos).ChildRules(shop =>
        {
            shop.RuleFor(s => s.CarrierCode)
                .NotEmpty().WithMessage("Please select a carrier.")
                .MaximumLength(10).WithMessage("Carrier code must not exceed 10 characters.");

            shop.RuleFor(s => s.ShopVoucherCode)
                .MaximumLength(50).WithMessage("Shop voucher code must not exceed 50 characters.")
                .When(s => !string.IsNullOrEmpty(s.ShopVoucherCode));
        });

        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("Please select a payment method.")
            .MaximumLength(50).WithMessage("Payment method must not exceed 50 characters.");

        RuleFor(x => x.PlatformVoucherCode)
            .MaximumLength(50).WithMessage("Platform voucher code must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.PlatformVoucherCode));
    }
}
