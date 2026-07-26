using ValidationException = Common.Domain.Exceptions.ValidationException;

namespace AuthService.Application.Features.Commands.Register.VerifyOtp;

public record VerifyOtpCommand(string Email, string Otp) : IRequest;

public class VerifyOtpCommandHandler(IOtpStore otpStore) : IRequestHandler<VerifyOtpCommand>
{
    public async Task Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        OtpVerifyResult result = await otpStore.VerifyCodeAsync(request.Email, request.Otp, cancellationToken);
        if (!result.IsValid || result.Role is null)
        {
            throw new ValidationException([
                new ValidationFailure("Otp", "The OTP code is invalid or has expired.")
            ]);
        }

        await otpStore.MarkVerifiedAsync(request.Email, result.Role, TimeSpan.FromMinutes(10), cancellationToken);
    }
}
