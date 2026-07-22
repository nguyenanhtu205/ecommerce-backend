namespace AuthService.Application.Features.Commands.Register.SetPassword;

public class Validator : AbstractValidator<SetPasswordCommand>
{
    private const string AllowedCharsPattern =
        @"^[A-Za-z0-9!@#$%^&*()_+\-=\[\]{};':""\\|,.<>/?]*$";

    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(16).WithMessage("Password must not exceed 16 characters.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches(AllowedCharsPattern)
            .WithMessage("Password may only contain letters, numbers, and supported special characters.");
    }
}
