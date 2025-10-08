using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Application.Contracts.Auth;
using Infrastructure.Auth.Configuration;
using Infrastructure.Auth.Exceptions;
using Microsoft.Extensions.Options;
using UnitTests.Factories;

using XJwtTokenService = Infrastructure.Auth.Services.JwtTokenService;
using static UnitTests.Infrastructure.Auth.JwtTokenServiceTest.JwtTokenServiceTestHelper;

namespace UnitTests.Infrastructure.Auth.JwtTokenServiceTest;

public class JwtTokenServiceTest
{
    private readonly JwtConfiguration _jwtConfiguration;
    
    private readonly XJwtTokenService _jwtTokenService;

    public JwtTokenServiceTest()
    {
        _jwtConfiguration = new JwtConfiguration
        {
            SecretKey = new string('a', 64),
            ExpirationMinutes = 5,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        
        var config = Options.Create(_jwtConfiguration);
        
        _jwtTokenService = new XJwtTokenService(config);
    }
    
    [Fact]
    public void CreateAccessToken_WhenValidUser_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        
        var handler = new JwtSecurityTokenHandler();

        // Act
        var token = _jwtTokenService.CreateAccessToken(
            uid: user.Id.ToString(),
            name: user.UserName,
            email: user.Email,
            roles: ["Admin", "User"],
            sid: null    
        );
        
        // Assert
        var jwt = handler.ReadJwtToken(token);
        var claims = jwt.Claims?.ToList();

        Assert.NotNull(jwt);
        Assert.NotNull(claims);
        Assert.NotEmpty(claims);
        
        AssertClaimsMatch(claims, TokenClaims.Subject, user.Id.ToString());
        AssertClaimsMatch(claims, TokenClaims.UserId, user.Id.ToString());
        AssertClaimsMatch(claims, TokenClaims.Name, user.UserName);
        AssertClaimsMatch(claims, TokenClaims.Email, user.Email);
        AssertClaimsMatch(claims, TokenClaims.Issuer, _jwtConfiguration.Issuer);
        AssertClaimsMatch(claims, TokenClaims.Audience, _jwtConfiguration.Audience);
        
        AssertRolesContain(claims, ["Admin", "User"]);
        
        AssertClaimExists(claims, TokenClaims.SessionId);
        AssertClaimExists(claims, TokenClaims.TokenId);
        AssertClaimExists(claims, TokenClaims.Expiration);
        AssertClaimExists(claims, TokenClaims.IssuedAt);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateAccessToken_WhenInvalidUserName_ShouldThrowJwtException(string? invalidUserName)
    {
        var user = TestDataFactory.CreateUser(userName: invalidUserName);

        Assert.Throws<JwtException>(() => _jwtTokenService.CreateAccessToken(
            uid: user.Id.ToString(),
            name: user.UserName,
            email: user.Email,
            roles: [],
            sid: null
        ));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateAccessToken_WithInvalidEmail_ShouldThrowJwtException(string? invalidEmail)
    {
        var user = TestDataFactory.CreateUser(email: invalidEmail);

        Assert.Throws<JwtException>(() => _jwtTokenService.CreateAccessToken(
            uid: user.Id.ToString(),
            name: user.UserName,
            email: user.Email,
            roles: [],
            sid: null    
        ));
    }
    
    [Fact]
    public void CreateRefreshToken_ShouldReturnToken()
    {
        var token = _jwtTokenService.CreateRefreshToken();
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        Assert.True(token.Length >= 40);
    }
    
    [Fact]
    public void GetClaimsFromToken_WhenValidToken_ShouldReturnClaims()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(TokenClaims.SessionId, Guid.NewGuid().ToString()),
            new Claim(TokenClaims.UserId, Guid.NewGuid().ToString()),
            new Claim(TokenClaims.Name, "Test User"),
            new Claim(TokenClaims.Email, "test@example.com"),
            new Claim(TokenClaims.Role, "User"),
            new Claim(TokenClaims.Role, "Admin")
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateJwtSecurityToken(subject: new ClaimsIdentity(claims));
        var tokenString = tokenHandler.WriteToken(token);
        
        // Act
        var result = _jwtTokenService.GetClaimsFromToken(tokenString);
        
        // Assert
        Assert.NotEmpty(result);
        
        AssertClaimsMatch(result, TokenClaims.SessionId, claims[0].Value);
        AssertClaimsMatch(result, TokenClaims.UserId, claims[1].Value);
        AssertClaimsMatch(result, TokenClaims.Name, claims[2].Value);
        AssertClaimsMatch(result, TokenClaims.Email, claims[3].Value);
        
        AssertRolesContain(result, ["Admin", "User"]);
    }
    
    [Fact]
    public void ValidateToken_WhenValidToken_ShouldReturnTrue()
    {
        // Arrange
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid);
        var token = GenerateJwt(_jwtConfiguration, claims);
        
        // Act
        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: true
        );
        
        // Assert
        Assert.True(result);
    }
    
    [Fact]
    public void ValidateToken_WhenExpiredButExpiryUnchecked_ShouldReturnTrue()
    {
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid);
        var expiredTime = DateTime.UtcNow.AddMinutes(-5);
        var token = GenerateJwt(_jwtConfiguration, claims, expires: expiredTime);

        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: false
        );

        Assert.True(result);
    }
    
    [Fact]
    public void ValidateToken_WhenNameIdClaimDoesNotMatchSubClaim_ShouldReturnFalse()
    {
        // Arrange
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid, "sub-will-not-match-this");
        var token = GenerateJwt(_jwtConfiguration, claims);
        
        // Act
        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: true
        );
        
        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public void ValidateToken_WhenMissingClaim_ShouldReturnFalse()
    {
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid);
        claims.RemoveAll(c => c.Type == TokenClaims.Email); // Delete a required claim
        
        var token = GenerateJwt(_jwtConfiguration, claims);

        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: true
        );

        Assert.False(result);
    }
    
    [Fact]
    public void ValidateToken_WhenNoRoles_ShouldReturnFalse()
    {
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid);
        claims.RemoveAll(c => c.Type == TokenClaims.Role); // Delete roles claim
        
        var token = GenerateJwt(_jwtConfiguration, claims);

        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: true
        );

        Assert.False(result);
    }
    
    [Fact]
    public void ValidateToken_WhenExpired_ShouldReturnFalse()
    {
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid);
        var expiredTime = DateTime.UtcNow.AddMinutes(-5); // Expired 5 mins ago
        var token = GenerateJwt(_jwtConfiguration, claims, expires: expiredTime);

        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: true
        );

        Assert.False(result);
    }
    
    [Fact]
    public void ValidateToken_WhenIssuerInvalid_ShouldReturnFalse()
    {
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid);

        var otherConfiguration = new JwtConfiguration
        {
            SecretKey = _jwtConfiguration.SecretKey,
            Issuer = "SomeOtherIssuer", // Invalid issuer
            Audience = _jwtConfiguration.Audience,
            ExpirationMinutes = _jwtConfiguration.ExpirationMinutes
        };
        var token = GenerateJwt(otherConfiguration, claims);

        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: true
        );

        Assert.False(result);
    }
    
    [Fact]
    public void ValidateToken_WhenSignatureInvalid_ShouldReturnFalse()
    {
        var uid = Guid.NewGuid().ToString("N");
        var claims = CreateValidClaims(uid);

        var otherConfiguration = new JwtConfiguration
        {
            SecretKey = new string('b', 64),
            Issuer = _jwtConfiguration.Issuer,
            Audience = _jwtConfiguration.Audience,
            ExpirationMinutes = _jwtConfiguration.ExpirationMinutes
        };
        
        var token = GenerateJwt(otherConfiguration, claims);

        var result = _jwtTokenService.ValidateToken(
            token: token, 
            claims: claims, 
            withExpiration: true
        );

        Assert.False(result);
    }
}