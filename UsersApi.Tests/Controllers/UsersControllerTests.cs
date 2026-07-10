using System.Net;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using UsersApi.Configurations.Mappers.Requests;
using UsersApi.Controllers;
using UsersApi.Services.Dto;
using UsersApi.Services.Interfaces;
using UsersApi.Tests.Helpers;

namespace UsersApi.Tests.Controllers;

public class UsersControllerTests
{
    private readonly IMapper _mapper = MapperFactory.Create();
    private readonly Mock<IUsersService> _usersServiceMock = new();

    private UsersController CreateController()
    {
        var controller = new UsersController(_mapper, _usersServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Connection.RemoteIpAddress = IPAddress.Loopback;
        controller.HttpContext.Request.Headers["User-Agent"] = "TestAgent";
        controller.HttpContext.Request.Headers["X-Device-ID"] = "test-device";
        return controller;
    }

    [Fact]
    public async Task Post_InvalidModelState_ReturnsBadRequest()
    {
        UsersController? controller = CreateController();
        controller.ModelState.AddModelError("Email", "Email is required");

        IActionResult? result = await controller.Post(new UserCreateDtoRequest());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Post_ValidRequest_ReturnsCreated()
    {
        UsersController? controller = CreateController();
        var request = new UserCreateDtoRequest
        {
            Name = "New",
            LastName = "User",
            Email = "new@example.com",
            Password = "Pass123!"
        };
        _usersServiceMock
            .Setup(s => s.CreateUserAsync(It.IsAny<UserEntity>()))
            .ReturnsAsync((UserEntity e) => e);

        IActionResult? result = await controller.Post(request);

        Assert.IsType<CreatedResult>(result);
        _usersServiceMock.Verify(s => s.CreateUserAsync(It.Is<UserEntity>(u => u.Email == "new@example.com")), Times.Once);
    }

    [Fact]
    public async Task GenerateToken_InvalidCredentials_ReturnsUnauthorized()
    {
        UsersController? controller = CreateController();
        _usersServiceMock
            .Setup(s => s.ValidateUserAsync("wrong@example.com", "bad"))
            .ReturnsAsync((UserEntity?)null);

        IActionResult? result = await controller.GenerateToken(new LoginRequest
        {
            Email = "wrong@example.com",
            Password = "bad"
        });

        UnauthorizedObjectResult? unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("The user does not exist", unauthorized.Value);
    }

    [Fact]
    public async Task GenerateToken_ValidCredentials_ReturnsToken()
    {
        UsersController? controller = CreateController();
        var user = new UserEntity
        {
            Id = 1,
            Name = "Test",
            Email = "test@example.com",
            StatusId = 2
        };
        _usersServiceMock
            .Setup(s => s.ValidateUserAsync("test@example.com", "pass"))
            .ReturnsAsync(user);
        _usersServiceMock
            .Setup(s => s.GenerateJwtToken(It.IsAny<UserLoginEntity>()))
            .Returns("jwt-token-value");

        IActionResult? result = await controller.GenerateToken(new LoginRequest
        {
            Email = "test@example.com",
            Password = "pass"
        });

        OkObjectResult? ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("jwt-token-value", ok.Value);
    }

    [Fact]
    public async Task GetUsers_ReturnsMappedUsers()
    {
        UsersController controller = CreateController();
        var users = new List<UserEntity>
        {
            new() { Name = "Alice", LastName = "A", Email = "alice@example.com" },
            new() { Name = "Bob", LastName = "B", Email = "bob@example.com" }
        };
        _usersServiceMock.Setup(s => s.GetUsers()).ReturnsAsync(users);

        IActionResult? result = await controller.GetUsers();

        OkObjectResult? ok = Assert.IsType<OkObjectResult>(result);
        List<UserResponse>? response = Assert.IsAssignableFrom<List<UserResponse>>(ok.Value);
        Assert.Equal(2, response.Count);
        Assert.Contains(response, u => u.Email == "alice@example.com");
    }

    [Fact]
    public async Task GetVersion_ReturnsVersionString()
    {
        UsersController controller = CreateController();

        IActionResult? result = await controller.GetVersion();

        OkObjectResult? ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("1.0.0", ok.Value);
    }
}
