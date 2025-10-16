using System.Security.Cryptography;
using System.Text;
using Application.Contracts.Services;
using Infrastructure.Hasher.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Hasher.Services;

public class AppHasher(IOptions<HasherConfiguration> config) : IHasher
{
    public string CreateHash(string value)
    {
        var hashKey = config.Value.Hashkey;
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(hashKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash);
    }
}