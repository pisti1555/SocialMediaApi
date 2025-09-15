using Application.Common.Mappings;
using AutoMapper;

namespace ApplicationUnitTests.Helpers;

internal static class MapperHelper
{
    internal static IMapper GetMapper()
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