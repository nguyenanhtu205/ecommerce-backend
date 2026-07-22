namespace AuthService.Application.Features.Commands.Login.LoginWithPassword;

public class Validator : AbstractValidator<LoginWithPasswordCommand>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is not in the correct format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must have at least 6 characters.");
    }
}
