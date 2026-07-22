using System.IdentityModel.Tokens.Jwt;

namespace AuthService.API.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            string? sub = httpContextAccessor.HttpContext?.User
                .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

            return Guid.TryParse(sub, out Guid userId) ? userId : null;
        }
    }
}
