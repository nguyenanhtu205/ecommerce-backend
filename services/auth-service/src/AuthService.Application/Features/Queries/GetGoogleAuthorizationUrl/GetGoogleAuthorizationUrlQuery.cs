using System.Security.Cryptography;

namespace AuthService.Application.Features.Queries.GetGoogleAuthorizationUrl;

public record GetGoogleAuthorizationUrlResponse(string Url);

public record GetGoogleAuthorizationUrlQuery : IRequest<GetGoogleAuthorizationUrlResponse>;

public class GetGoogleAuthorizationUrl(
    IGoogleOAuthService googleOAuthService,
    IOAuthStateStore stateStore
) : IRequestHandler<GetGoogleAuthorizationUrlQuery, GetGoogleAuthorizationUrlResponse>
{
    private static readonly TimeSpan StateTtl = TimeSpan.FromMinutes(10);

    public async Task<GetGoogleAuthorizationUrlResponse> Handle(GetGoogleAuthorizationUrlQuery request,
        CancellationToken cancellationToken)
    {
        string state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        await stateStore.SetStateAsync(state, StateTtl, cancellationToken);

        string url = googleOAuthService.GetAuthorizationUrl(state);

        return new GetGoogleAuthorizationUrlResponse(url);
    }
}
