using Microsoft.AspNetCore.Identity;

namespace Persistence.Auth.Models;

public class AppIdentityRole : IdentityRole<Guid>
{
    public AppIdentityRole() : base() { }
    public AppIdentityRole(string roleName) : base(roleName) { }
}