using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Options;
using Moq;
using UsersApi.Configurations.ClientsDB.Mongo;
using UsersApi.Repository.SqlServer;
using UsersApi.Repository.SqlServer.Dto;
using UsersApi.Services.Dto;
using UsersApi.Services.Exceptions;
using UsersApi.Services.Implementations;
using UsersApi.Services.Interfaces;
using UsersApi.Tests.Helpers;

namespace UsersApi.Tests.Services;

public class UsersServiceTests
{
    private readonly Mock<ISessionsService> _sessionsServiceMock = new();
    private readonly SecurityAuth _jwtSettings = new()
    {
        Authority = "https://test-authority",
        Audience = "test-audience",
        SecretKey = "test-secret-key-with-enough-length-for-hmac"
    };

    private UsersService CreateService(UserDbContext context) =>
        new(context, Options.Create(_jwtSettings), _sessionsServiceMock.Object);

    [Fact]
    public async Task CreateUserAsync_WithPassword_HashesAndPersistsUser()
    {
        await using UserDbContext? context = TestDbContextFactory.Create(nameof(CreateUserAsync_WithPassword_HashesAndPersistsUser));
        UsersService? service = CreateService(context);
        var entity = new UserEntity
        {
            Name = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "SecurePass123!"
        };

        UserEntity? result = await service.CreateUserAsync(entity);

        Assert.Equal("john@example.com", result.Email);
        UserDtoContext? savedUser = context.Users.Single();
        Assert.Equal("John", savedUser.Name);
        Assert.Equal("Doe", savedUser.LastName);
        Assert.Equal(savedUser.Id, result.Id);
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
    public async Task GenerateJwtTokenAsync_ReturnsValidTokenWithExpectedClaims()
    {
        await using var context = TestDbContextFactory.Create(nameof(GenerateJwtTokenAsync_ReturnsValidTokenWithExpectedClaims));
        var service = CreateService(context);
        var userLogin = new UserLoginEntity
        {
            Id = 1,
            Name = "User",
            Email = "user@example.com",
            Device = new UserDevice { Ip = "127.0.0.1", DeviceId = "device-1", DeviceName = "Test" }
        };

        var tokenString = await service.GenerateJwtTokenAsync(userLogin);

        Assert.False(string.IsNullOrWhiteSpace(tokenString));
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(tokenString);
        Assert.Equal(_jwtSettings.Authority, token.Issuer);
        Assert.Contains(_jwtSettings.Audience, token.Audiences);
        Assert.Equal("1", token.Subject);
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "user@example.com");
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == "User");
        Assert.Contains(token.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
        _sessionsServiceMock.Verify(
            s => s.CreateSessionAsync(userLogin, It.IsAny<DateTime>(), tokenString),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_DuplicateEmail_ThrowsDuplicateEmailException()
    {
        await using var context = TestDbContextFactory.Create(nameof(CreateUserAsync_DuplicateEmail_ThrowsDuplicateEmailException));
        var service = CreateService(context);
        await service.CreateUserAsync(new UserEntity { Name = "First", Email = "dup@example.com", Password = "Password1!" });

        await Assert.ThrowsAsync<DuplicateEmailException>(() =>
            service.CreateUserAsync(new UserEntity { Name = "Second", Email = "dup@example.com", Password = "Password2!" }));

        Assert.Single(context.Users);
    }

    [Fact]
    public async Task GenerateJwtTokenAsync_WhenSessionStoreFails_PropagatesException()
    {
        await using var context = TestDbContextFactory.Create(nameof(GenerateJwtTokenAsync_WhenSessionStoreFails_PropagatesException));
        _sessionsServiceMock
            .Setup(s => s.CreateSessionAsync(It.IsAny<UserLoginEntity>(), It.IsAny<DateTime>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("mongo down"));
        var service = CreateService(context);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.GenerateJwtTokenAsync(new UserLoginEntity
        {
            Id = 1,
            Email = "user@example.com",
            Device = new UserDevice()
        }));
    }
}
