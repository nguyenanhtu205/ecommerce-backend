namespace AuthService.Application.Features.Commands.RefreshUserToken;

public class Validator : AbstractValidator<RefreshUserTokenCommand>
{
    public Validator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.");
    }
}
