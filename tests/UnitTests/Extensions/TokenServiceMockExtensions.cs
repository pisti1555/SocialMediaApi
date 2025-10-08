using System.Security.Claims;
using Application.Contracts.Services;
using Moq;

namespace UnitTests.Extensions;

public static class TokenServiceMockExtensions
{
    public static void SetupCreateAccessToken(this Mock<ITokenService> tokenServiceMock, string? returnValue = null)
    {
        tokenServiceMock
            .Setup(x => x.CreateAccessToken(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<string?>()
            )).Returns(returnValue ?? "generated-access-token");
    }

    public static void SetupCreateRefreshToken(this Mock<ITokenService> tokenServiceMock, string? returnValue = null)
    {
        tokenServiceMock
            .Setup(x => x.CreateRefreshToken())
            .Returns(returnValue ?? "generated-refresh-token");
    }

    public static void SetupValidateToken(this Mock<ITokenService> tokenServiceMock, bool isValid)
    {
        tokenServiceMock.Setup(x => x.ValidateToken(It.IsAny<string>(), It.IsAny<List<Claim>>(), false))
            .Returns(isValid);
    }

    public static void SetupGetClaimsFromToken(this Mock<ITokenService> tokenServiceMock, List<Claim> claims)
    {
        tokenServiceMock.Setup(x => x.GetClaimsFromToken(It.IsAny<string>()))
            .Returns(claims);
    }
}