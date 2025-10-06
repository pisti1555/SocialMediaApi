using Application.Contracts.Services;
using Domain.Users;
using Moq;

namespace UnitTests.Extensions;

public static class TokenServiceMockExtensions
{
    public static void SetupCreateToken(this Mock<ITokenService> tokenServiceMock)
    {
        tokenServiceMock
            .Setup(x => x.CreateToken(It.IsAny<AppUser>(), It.IsAny<IEnumerable<string>>()))
            .Returns("generated-jwt-token");
    }
}