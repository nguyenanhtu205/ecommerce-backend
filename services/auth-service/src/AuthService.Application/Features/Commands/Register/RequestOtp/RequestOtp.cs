using ValidationException = Common.Domain.Exceptions.ValidationException;

namespace AuthService.Application.Features.Commands.Register.RequestOtp;

public record RequestOtpCommand(string Email, string Role) : IRequest;

public class RequestOtpCommandHandler(
    IApplicationDbContext context,
    IOtpStore otpStore,
    ITopicProducer<OtpRequested> producer) : IRequestHandler<RequestOtpCommand>
{
    private static readonly Random Random = new();

    public async Task Handle(RequestOtpCommand request, CancellationToken cancellationToken)
    {
        bool hasPassword = await context.Users
            .Where(u => u.Email == request.Email)
            .Join(context.AuthCredentials, u => u.Id, c => c.UserId, (u, c) => u)
            .AnyAsync(cancellationToken);

        if (hasPassword)
        {
            throw new ConflictException("Email is already registered.");
        }

        bool roleExists = await context.Roles.AnyAsync(r => r.Name == request.Role, cancellationToken);
        if (!roleExists)
        {
            throw new ValidationException([
                new ValidationFailure("Role", $"Role '{request.Role}' does not exist.")
            ]);
        }

        string code = Random.Next(0, 1_000_000).ToString("D6");

        await otpStore.SetCodeAsync(request.Email, code, request.Role, TimeSpan.FromMinutes(5), cancellationToken);

        await producer.Produce(
            new OtpRequested(request.Email, code, "register", DateTimeOffset.UtcNow), cancellationToken);
    }
}
