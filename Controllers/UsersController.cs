using UsersApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using UsersApi.Configurations.Mappers;
using UsersApi.Configurations.Mappers.Requests;
using UsersApi.Configurations.Mappers.Responses;
using UsersApi.Services.Exceptions;

namespace UsersApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _userService;

        public UsersController(IUsersService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserCreateDtoRequest dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            bool passwordGenerated = string.IsNullOrWhiteSpace(dto.Password);
            UserEntity userEntity = dto.ToEntity();

            try
            {
                userEntity = await _userService.CreateUserAsync(userEntity);
            }
            catch (DuplicateEmailException ex)
            {
                return Conflict(ex.Message);
            }

            var response = new UserCreatedResponse
            {
                Id = userEntity.Id,
                Email = userEntity.Email,
                GeneratedPassword = passwordGenerated ? userEntity.Password : null
            };
            return Created($"/api/v1/users/{userEntity.Id}", response);
        }

        [HttpPost("generate-token")]
        public async Task<IActionResult> GenerateToken([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            UserEntity? user = await _userService.ValidateUserAsync(loginRequest.Email, loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("The user does not exist");
            }

            var headers = HttpContext.Request.Headers;

            UserLoginEntity userLogin = user.ToLoginEntity(new UserDevice
            {
                Ip = userIp,
                DeviceName = headers["User-Agent"].ToString() ?? "Unknown",
                DeviceId = headers["X-Device-ID"].ToString() ?? "Unknown"
            });

            string tokenString = await _userService.GenerateJwtTokenAsync(userLogin);
            return Ok(tokenString);
        }

        [Authorize(AuthenticationSchemes = "SecurityAuth")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<UserEntity>? users = await _userService.GetUsers();
            List<UserResponse> response = users.Select(u => u.ToResponse()).ToList();
            return Ok(response);
        }

        [Authorize(AuthenticationSchemes = "SecurityAuth")]
        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            return Ok("1.0.0");
        }

    }
}