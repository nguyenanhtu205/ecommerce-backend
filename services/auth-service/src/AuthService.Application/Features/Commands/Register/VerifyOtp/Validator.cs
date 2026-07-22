namespace AuthService.Application.Features.Commands.Register.VerifyOtp;

public class Validator : AbstractValidator<VerifyOtpCommand>
{
    public Validator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email format is invalid.");

        RuleFor(x => x.Otp)
            .NotEmpty().Length(6);
    }
}
