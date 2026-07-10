using AutoMapper;
using UsersApi.Configurations.Mappers;

namespace UsersApi.Tests.Helpers;

public static class MapperFactory
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<ContractMapping>());
        return config.CreateMapper();
    }
}
