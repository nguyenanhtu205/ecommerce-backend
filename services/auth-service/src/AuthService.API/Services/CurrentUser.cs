using System.Text;

namespace AuthService.API.Services;

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

    public Guid? ShopId
    {
        get
        {
            string? shopId = httpContextAccessor.HttpContext?
                .Request.Headers["X-Shop-Id"].FirstOrDefault();

            return Guid.TryParse(shopId, out Guid id) ? id : null;
        }
    }

    public string? ShopName
    {
        get
        {
            string? encoded = httpContextAccessor.HttpContext?
                .Request.Headers["X-Shop-Name"].FirstOrDefault();

            if (string.IsNullOrEmpty(encoded))
            {
                return null;
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(encoded);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
