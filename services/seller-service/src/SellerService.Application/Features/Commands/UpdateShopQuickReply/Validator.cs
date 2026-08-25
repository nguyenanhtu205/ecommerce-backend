namespace SellerService.Application.Features.Commands.UpdateShopQuickReply;

public class Validator : AbstractValidator<UpdateShopQuickReplyCommand>
{
    public Validator()
    {
        RuleFor(x => x.Id)
            .NotNull().WithMessage("Id is required.");

        RuleFor(x => x.Title)
            .NotNull().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must be less than 255 characters.");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("Content is required.");
    }
}
