namespace AuthService.Application.Features.Commands.Register.RequestOtp;

public class Validator : AbstractValidator<RequestOtpCommand>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");
    }
}
