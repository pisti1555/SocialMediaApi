using Application.Contracts.Services;
using Moq;

namespace UnitTests.Extensions;

public static class CacheServiceMockExtensions
{
    // Setup
    public static void SetupCache<T>(this Mock<ICacheService> cacheServiceMock, string cacheKey, T? cachedData)
    {
        cacheServiceMock.Setup(x => x.GetAsync<T>(cacheKey, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(cachedData);
    }

    // Verifications
    public static void VerifyCacheHit<T>(this Mock<ICacheService> cacheServiceMock, string? cacheKey = null, bool happened = true)
    {
        //cacheServiceMock.Verify(x => x.GetAsync<T>(cacheKey ?? It.IsAny<string>(), It.IsAny<CancellationToken>()), happened ? Times.Once : Times.Never);
        //return;
        if (string.IsNullOrEmpty(cacheKey))
        {
            cacheServiceMock.Verify(x => x.GetAsync<T>(It.IsAny<string>(), It.IsAny<CancellationToken>()), happened ? Times.Once : Times.Never);
        }
        else
        {
            cacheServiceMock.Verify(x => x.GetAsync<T>(cacheKey, It.IsAny<CancellationToken>()), happened ? Times.Once : Times.Never);
        }
    }
    
    public static void VerifyCacheSet<T>(this Mock<ICacheService> cacheServiceMock, string? cacheKey = null, bool happened = true)
    {
        if (string.IsNullOrEmpty(cacheKey))
        {
            cacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<T>(), It.IsAny<CancellationToken>()), happened ? Times.Once : Times.Never);
        }
        else
        {
            cacheServiceMock.Verify(x => x.SetAsync(cacheKey, It.IsAny<T>(), It.IsAny<CancellationToken>()), happened ? Times.Once : Times.Never);
        }
    }
    
    public static void VerifyCacheRemove(this Mock<ICacheService> cacheServiceMock, string? cacheKey = null, bool happened = true)
    {
        if (string.IsNullOrEmpty(cacheKey))
        {
            cacheServiceMock.Verify(x => x.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), happened ? Times.Once : Times.Never);
        }
        else
        {
            cacheServiceMock.Verify(x => x.RemoveAsync(cacheKey, It.IsAny<CancellationToken>()), happened ? Times.Once : Times.Never);
        }
    }
}