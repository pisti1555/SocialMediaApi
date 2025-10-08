using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Auth.Models;

public class AppIdentityUser : IdentityUser<Guid>
{
    public ICollection<Token> Tokens { get; private set; } = [];
}