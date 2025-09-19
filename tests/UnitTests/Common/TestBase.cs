using Application.Common.Mappings;
using Application.Contracts.Services;
using AutoMapper;
using Moq;

namespace UnitTests.Common;

public abstract class TestBase
{
    protected readonly IMapper Mapper = SetupMapper();
    protected readonly Mock<ICacheService> CacheServiceMock = new();
    
    private static IMapper SetupMapper()
    {
        var mapperConfig = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserProfile>();
            cfg.AddProfile<PostProfile>();
        });
        
        mapperConfig.AssertConfigurationIsValid();

        return mapperConfig.CreateMapper();
    }
}