using Application.Common.Mappings;
using AutoMapper;

namespace ApplicationUnitTests.Common;

internal static class TestMapperSetup
{
    internal static IMapper SetupMapper()
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