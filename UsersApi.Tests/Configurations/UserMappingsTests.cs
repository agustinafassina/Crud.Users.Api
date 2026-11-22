using UsersApi.Configurations.Mappers;
using UsersApi.Configurations.Mappers.Requests;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;

namespace UsersApi.Tests.Configurations;

public class UserMappingsTests
{
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

        UserEntity? entity = request.ToEntity();

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
            LastName = "Roe",
            Email = "jane@example.com",
            Password = "hashed-password",
            StatusId = 2,
            CreatedDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        UserEntity? entity = dto.ToEntity();

        Assert.Equal(10, entity.Id);
        Assert.Equal("Jane", entity.Name);
        Assert.Equal("jane@example.com", entity.Email);
        Assert.Equal("Roe", entity.LastName);
        Assert.Equal(2, entity.StatusId);
        Assert.Equal(dto.CreatedDate, entity.CreatedDate);
        Assert.Null(entity.Password);
    }

    [Fact]
    public void Map_UserEntity_ToUserLoginEntity_IncludesDevice()
    {
        var entity = new UserEntity { Id = 5, Name = "Bob", Email = "bob@example.com", StatusId = 2 };
        var device = new UserDevice { Ip = "127.0.0.1", DeviceId = "d1", DeviceName = "agent" };

        UserLoginEntity login = entity.ToLoginEntity(device);

        Assert.Equal(5, login.Id);
        Assert.Equal("bob@example.com", login.Email);
        Assert.Same(device, login.Device);
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

        UserResponse? response = entity.ToResponse();

        Assert.Equal("Alice", response.Name);
        Assert.Equal("Smith", response.LastName);
        Assert.Equal("alice@example.com", response.Email);
    }
}
