using Domain.Users;

namespace Application.Contracts.Services;

public interface ITokenService
{
    public string CreateToken(AppUser user, IEnumerable<string> roles);
}