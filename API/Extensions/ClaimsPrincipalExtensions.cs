using System.Security.Claims;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions // Acces these methods via the Claims Principal e.g. User.GetUsername()
{
    public static string GetUsername(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Name)?.Value;
    }
    
    public static string GetUserId(this ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
