using System.Security.Cryptography;
using System.Text;
using Application.Contracts.Services;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Hasher;

public class AppHasher(IConfiguration config) : IHasher
{
    public string CreateHash(string value)
    {
        var hashKey = config["HashKey"] ?? throw new InvalidOperationException("Hash key is missing.");
        
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(hashKey));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
        return Convert.ToBase64String(hash);
    }
}