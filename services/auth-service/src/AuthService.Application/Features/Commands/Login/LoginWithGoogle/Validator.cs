namespace AuthService.Application.Features.Commands.Login.LoginWithGoogle;

public class Validator : AbstractValidator<LoginWithGoogleCommand>
{
    public Validator()
    {
        RuleFor(x => x.AuthorizationCode)
            .NotEmpty().WithMessage("Authorization code is required.");

        RuleFor(x => x.State)
            .NotEmpty().WithMessage("State is required.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.");
    }
}
