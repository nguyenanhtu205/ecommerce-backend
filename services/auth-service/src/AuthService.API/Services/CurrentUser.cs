using System.Security.Claims;

namespace AuthService.API.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            string? sub = httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(sub, out Guid userId) ? userId : null;
        }
    }

    public bool IsSeller => httpContextAccessor.HttpContext?.User.IsInRole("seller") ?? false;
}
