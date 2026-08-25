namespace SellerService.Application.Features.Commands.CreateShopQuickReply;

public class Validator : AbstractValidator<CreateShopQuickReplyCommand>
{
    public Validator()
    {
        RuleFor(x => x.Title)
            .NotNull().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must be less than 255 characters.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Content is required.");
    }
}
