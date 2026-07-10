using AutoMapper;
using UsersApi.Configurations.Mappers.Requests;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;
using UsersApi.Tests.Helpers;

namespace UsersApi.Tests.Configurations;

public class ContractMappingTests
{
    private readonly IMapper _mapper = MapperFactory.Create();

    [Fact]
    public void Map_UserCreateDtoRequest_ToUserEntity()
    {
        var request = new UserCreateDtoRequest
        {
            Name = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "secret"
        };

        UserEntity? entity = _mapper.Map<UserEntity>(request);

        Assert.Equal("John", entity.Name);
        Assert.Equal("Doe", entity.LastName);
        Assert.Equal("john@example.com", entity.Email);
        Assert.Equal("secret", entity.Password);
    }

    [Fact]
    public void Map_UserDtoContext_ToUserEntity()
    {
        var dto = new UserDtoContext
        {
            Id = 10,
            Name = "Jane",
            Email = "jane@example.com",
            StatusId = 2,
            CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        UserEntity? entity = _mapper.Map<UserEntity>(dto);

        Assert.Equal(10, entity.Id);
        Assert.Equal("Jane", entity.Name);
        Assert.Equal("jane@example.com", entity.Email);
        Assert.Equal(2, entity.StatusId);
    }

    [Fact]
    public void Map_UserEntity_ToUserResponse()
    {
        var entity = new UserEntity
        {
            Name = "Alice",
            LastName = "Smith",
            Email = "alice@example.com"
        };

        UserResponse? response = _mapper.Map<UserResponse>(entity);

        Assert.Equal("Alice", response.Name);
        Assert.Equal("Smith", response.LastName);
        Assert.Equal("alice@example.com", response.Email);
    }
}
