using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Auth.Models;

public class AppIdentityRole : IdentityRole<Guid>
{
    public AppIdentityRole() : base() { }
    public AppIdentityRole(string roleName) : base(roleName) { }
}