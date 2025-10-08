using Domain.Common.Exceptions.CustomExceptions;

namespace Infrastructure.Auth.Models;

public class Token
{
    public string Id { get; private set; }

    public Guid UserId { get; private set; }
    public AppIdentityUser User { get; private set; }

    public string JtiHash { get; private set; }
    public string RefreshTokenHash { get; private set; }
    
    public bool IsLongSession { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime MaxExpiry { get; private set; }

    public DateTime LastSeenAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Token()
    {
        Id = string.Empty;
        JtiHash = string.Empty;
        RefreshTokenHash = string.Empty;
        User = null!;
        CreatedAt = DateTime.UtcNow;
        LastSeenAt = DateTime.UtcNow;
    }

    public static Token CreateToken(
        string sessionId, Guid userId, 
        string jtiHash, string refreshTokenHash, bool isLongSession
    )
    {
        var token = new Token
        {
            Id = sessionId,
            UserId = userId,
            JtiHash = jtiHash,
            RefreshTokenHash = refreshTokenHash,
            IsLongSession = isLongSession,
            ExpiresAt = DateTime.UtcNow.AddHours(isLongSession ? 24*14 : 12),
            MaxExpiry = DateTime.UtcNow.AddDays(90)
        };

        return token;
    }

    public void Refresh(string jtiHash, string refreshTokenHash)
    {
        var utcNow = DateTime.UtcNow;

        if (utcNow >= ExpiresAt || utcNow >= MaxExpiry)
        {
            throw new UnauthorizedException("Token is expired.");
        }

        JtiHash = jtiHash;
        RefreshTokenHash = refreshTokenHash;

        var nextExpiry = utcNow.AddHours(IsLongSession ? 24*14 : 12);

        ExpiresAt = nextExpiry >= MaxExpiry ? MaxExpiry : nextExpiry;
        LastSeenAt = utcNow;
    }

    public bool IsExpired() 
    {
        var utcNow = DateTime.UtcNow;
        return utcNow >= ExpiresAt || utcNow >= MaxExpiry;
    }
}