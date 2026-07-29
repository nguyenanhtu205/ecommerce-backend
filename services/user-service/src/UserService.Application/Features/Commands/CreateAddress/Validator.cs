namespace UserService.Application.Features.Commands.CreateAddress;

public class Validator : AbstractValidator<CreateAddressCommand>
{
    private const string PhonePattern = @"^[0-9+\-\s]*$";

    public Validator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(255).WithMessage("Full name must not exceed 255 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .MaximumLength(20).WithMessage("Phone must not exceed 20 characters.")
            .Matches(PhonePattern).WithMessage("Phone format is invalid.");

        RuleFor(x => x.Province)
            .NotEmpty().WithMessage("Province is required.")
            .MaximumLength(255).WithMessage("Province must not exceed 255 characters.");

        RuleFor(x => x.Ward)
            .NotEmpty().WithMessage("Ward is required.")
            .MaximumLength(255).WithMessage("Ward must not exceed 255 characters.");

        RuleFor(x => x.AddressDetail)
            .NotEmpty().WithMessage("Address detail is required.")
            .MaximumLength(500).WithMessage("Address detail must not exceed 500 characters.");

        RuleFor(x => x.FullAddressText)
            .NotEmpty().WithMessage("Full address text is required.")
            .MaximumLength(1000).WithMessage("Full address text must not exceed 1000 characters.");

        RuleFor(x => x.AddressType)
            .IsInEnum().WithMessage("Address type is invalid.");
    }
}
