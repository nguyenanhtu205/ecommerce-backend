using System.Security.Claims;

namespace InventoryService.API.Services;

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
}

// public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
// {
//     public Guid? UserId
//     {
//         get
//         {
//             string? userId = httpContextAccessor.HttpContext?.Request
//                 .Headers["X-User-Id"].FirstOrDefault();
//
//             return Guid.TryParse(userId, out Guid id) ? id : null;
//         }
//     }
// }
