using System.IdentityModel.Tokens.Jwt;
using Infrastructure.Auth.Configuration;
using Infrastructure.Auth.Exceptions;
using Infrastructure.Auth.Services;
using Microsoft.Extensions.Options;
using UnitTests.Factories;

namespace UnitTests.Infrastructure.Auth;

public class JwtTokenServiceTest
{
    private readonly JwtConfiguration _jwtConfiguration;
    
    private readonly JwtTokenService _jwtTokenService;

    public JwtTokenServiceTest()
    {
        _jwtConfiguration = new JwtConfiguration
        {
            SecretKey = new string('a', 64),
            ExpirationMinutes = 10,
            Issuer = "TestIssuer",
            Audience = "TestAudience"
        };
        
        var config = Options.Create(_jwtConfiguration);
        
        _jwtTokenService = new JwtTokenService(config);
    }
    
    [Fact]
    public void CreateToken_WhenValidUser_ShouldContainCorrectClaims()
    {
        // Arrange
        var user = TestDataFactory.CreateUser();
        
        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();

        // Act
        var token = _jwtTokenService.CreateToken(user, ["Admin", "User"]);
        
        // Assert
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(user.Id.ToString(), jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);
        Assert.Equal(user.Id.ToString(), jwt.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value);
        Assert.Equal(user.UserName, jwt.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value);
        Assert.Equal(user.Email, jwt.Claims.FirstOrDefault(c => c.Type == "email")?.Value);
        Assert.Equal(_jwtConfiguration.Issuer, jwt.Claims.FirstOrDefault(c => c.Type == "iss")?.Value);
        Assert.Equal(_jwtConfiguration.Audience, jwt.Claims.FirstOrDefault(c => c.Type == "aud")?.Value);

        var roleClaims = jwt.Claims.Where(c => c.Type == "role").Select(c => c.Value).ToList();
        Assert.Contains("Admin", roleClaims);
        Assert.Contains("User", roleClaims);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateToken_WhenInvalidUserName_ShouldThrowJwtException(string? invalidUserName)
    {
        var user = TestDataFactory.CreateUser(userName: invalidUserName);

        Assert.Throws<JwtException>(() => _jwtTokenService.CreateToken(user, []));
    }
    
    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateToken_WithInvalidEmail_ShouldThrowJwtException(string? invalidEmail)
    {
        var user = TestDataFactory.CreateUser(email: invalidEmail);

        Assert.Throws<JwtException>(() => _jwtTokenService.CreateToken(user, []));
    }
}