using System.Security.Claims;
using Domain.Common.Exceptions.CustomExceptions;

namespace API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return id ?? throw new UnauthorizedException("Invalid authorization token.");
    }
    
    public static string GetUserName(this ClaimsPrincipal user)
    {
        var username = user.FindFirstValue(ClaimTypes.Name);
        return username ?? throw new UnauthorizedException("Invalid authorization token.");
    }
    
    public static string GetUserEmail(this ClaimsPrincipal user)
    {
        var email = user.FindFirstValue(ClaimTypes.Email);
        return email ?? throw new UnauthorizedException("Invalid authorization token.");
    }
}