using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using Microsoft.Extensions.Options;
using Moq;
using UsersApi.Configurations.ClientsDB.Mongo;
using UsersApi.Repository.SqlServer;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;
using UsersApi.Services.Implementations;
using UsersApi.Services.Interfaces;
using UsersApi.Tests.Helpers;

namespace UsersApi.Tests.Services;

public class UsersServiceTests
{
    private readonly IMapper _mapper = MapperFactory.Create();
    private readonly Mock<ISessionsService> _sessionsServiceMock = new();
    private readonly SecurityAuth _jwtSettings = new()
    {
        Authority = "https://test-authority",
        Audience = "test-audience",
        SecretKey = "test-secret-key-with-enough-length-for-hmac"
    };

    private UsersService CreateService(UserDbContext context) =>
        new(context, _mapper, Options.Create(_jwtSettings), _sessionsServiceMock.Object);

    [Fact]
    public async Task CreateUserAsync_WithPassword_HashesAndPersistsUser()
    {
        await using UserDbContext? context = TestDbContextFactory.Create(nameof(CreateUserAsync_WithPassword_HashesAndPersistsUser));
        UsersService? service = CreateService(context);
        var entity = new UserEntity
        {
            Name = "John",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };

        UserEntity? result = await service.CreateUserAsync(entity);

        Assert.Equal("john@example.com", result.Email);
        UserDtoContext? savedUser = context.Users.Single();
        Assert.Equal("John", savedUser.Name);
        Assert.Equal(2, savedUser.StatusId);
        Assert.NotEqual("SecurePass123!", savedUser.Password);
        Assert.True(BCrypt.Net.BCrypt.Verify("SecurePass123!", savedUser.Password));
    }

    [Fact]
    public async Task CreateUserAsync_WithoutPassword_GeneratesRandomPassword()
    {
        await using UserDbContext? context = TestDbContextFactory.Create(nameof(CreateUserAsync_WithoutPassword_GeneratesRandomPassword));
        UsersService service = CreateService(context);
        var entity = new UserEntity
        {
            Name = "Jane",
            Email = "jane@example.com",
            Password = ""
        };

        UserEntity? result = await service.CreateUserAsync(entity);

        Assert.False(string.IsNullOrWhiteSpace(result.Password));
        Assert.Equal(16, result.Password!.Length);
        UserDtoContext? savedUser = context.Users.Single();
        Assert.True(BCrypt.Net.BCrypt.Verify(result.Password, savedUser.Password));
    }

    [Fact]
    public async Task GetUsers_ReturnsMappedUsers()
    {
        await using UserDbContext? context = TestDbContextFactory.Create(nameof(GetUsers_ReturnsMappedUsers));
        context.Users.AddRange(
            new UserDtoContext
            {
                Name = "Alice",
                Email = "alice@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("pass"),
                CreatedDate = DateTime.UtcNow,
                StatusId = 2
            },
            new UserDtoContext
            {
                Name = "Bob",
                Email = "bob@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("pass"),
                CreatedDate = DateTime.UtcNow,
                StatusId = 2
            });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var users = await service.GetUsers();

        Assert.Equal(2, users.Count);
        Assert.Contains(users, u => u.Email == "alice@example.com");
        Assert.Contains(users, u => u.Email == "bob@example.com");
    }

    [Fact]
    public async Task ValidateUserAsync_ValidCredentials_ReturnsUser()
    {
        const string password = "ValidPass123!";
        await using var context = TestDbContextFactory.Create(nameof(ValidateUserAsync_ValidCredentials_ReturnsUser));
        context.Users.Add(new UserDtoContext
        {
            Name = "Test",
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedDate = DateTime.UtcNow,
            StatusId = 2
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var user = await service.ValidateUserAsync("test@example.com", password);

        Assert.NotNull(user);
        Assert.Equal("test@example.com", user!.Email);
    }

    [Fact]
    public async Task ValidateUserAsync_InvalidPassword_ReturnsNull()
    {
        await using var context = TestDbContextFactory.Create(nameof(ValidateUserAsync_InvalidPassword_ReturnsNull));
        context.Users.Add(new UserDtoContext
        {
            Name = "Test",
            Email = "test@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("correct"),
            CreatedDate = DateTime.UtcNow,
            StatusId = 2
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var user = await service.ValidateUserAsync("test@example.com", "wrong");

        Assert.Null(user);
    }

    [Fact]
    public async Task ValidateUserAsync_DisabledUser_ReturnsNull()
    {
        await using var context = TestDbContextFactory.Create(nameof(ValidateUserAsync_DisabledUser_ReturnsNull));
        context.Users.Add(new UserDtoContext
        {
            Name = "Disabled",
            Email = "disabled@example.com",
            Password = BCrypt.Net.BCrypt.HashPassword("pass"),
            CreatedDate = DateTime.UtcNow,
            StatusId = 1
        });
        await context.SaveChangesAsync();

        var service = CreateService(context);
        var user = await service.ValidateUserAsync("disabled@example.com", "pass");

        Assert.Null(user);
    }

    [Fact]
    public void GenerateJwtToken_ReturnsValidTokenWithExpectedClaims()
    {
        using var context = TestDbContextFactory.Create(nameof(GenerateJwtToken_ReturnsValidTokenWithExpectedClaims));
        var service = CreateService(context);
        var userLogin = new UserLoginEntity
        {
            Id = 1,
            Email = "user@example.com",
            Device = new UserDevice { Ip = "127.0.0.1", DeviceId = "device-1", DeviceName = "Test" }
        };

        var tokenString = service.GenerateJwtToken(userLogin);

        Assert.False(string.IsNullOrWhiteSpace(tokenString));
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        Assert.Equal(_jwtSettings.Authority, token.Issuer);
        Assert.Contains(_jwtSettings.Audience, token.Audiences);
        _sessionsServiceMock.Verify(
            s => s.CreateSessionAsync(userLogin, It.IsAny<DateTime>(), tokenString),
            Times.Once);
    }
}
