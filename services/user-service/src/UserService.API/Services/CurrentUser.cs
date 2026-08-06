namespace UserService.API.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public Guid? UserId
    {
        get
        {
            string? userId = httpContextAccessor.HttpContext?
                .Request.Headers["X-User-Id"].FirstOrDefault();

            return Guid.TryParse(userId, out Guid id) ? id : null;
        }
    }

    public bool IsSeller
    {
        get
        {
            string? roles = httpContextAccessor.HttpContext?
                .Request.Headers["X-User-Roles"].FirstOrDefault();

            return roles?.Split(',').Contains("seller") ?? false;
        }
    }
}
