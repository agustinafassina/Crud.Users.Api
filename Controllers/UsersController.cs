using AutoMapper;
using UsersApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Services.Dto;
using Microsoft.AspNetCore.Authorization;
using UsersApi.Configurations.Mappers.Requests;

namespace UsersApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUsersService _userService;

        public UsersController(IMapper mapper, IUsersService userService)
        {
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserCreateDtoRequest dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                UserEntity userEntity = _mapper.Map<UserEntity>(dto);
                await _userService.CreateUserAsync(userEntity);

                return Created();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost("generate-token")]
        public async Task<IActionResult> GenerateToken([FromBody] LoginRequest loginRequest)
        {
            var userIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            UserEntity user = await _userService.ValidateUserAsync(loginRequest.Email, loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("The user does not exist");
            }

            var headers = HttpContext.Request.Headers;

            UserLoginEntity userLogin = _mapper.Map<UserLoginEntity>(user);
            userLogin.Device = new UserDevice
            {
                Ip = userIp,
                DeviceName = headers["User-Agent"].ToString() ?? "Unknown",
                DeviceId = headers["X-Device-ID"].ToString() ?? "Unknown"
            };

            string tokenString = _userService.GenerateJwtToken(userLogin);
            return Ok(tokenString);
        }

        [Authorize(AuthenticationSchemes = "SecurityAuth")]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<UserEntity>? users = await _userService.GetUsers();
            List<UserResponse> response = _mapper.Map<List<UserResponse>>(users);
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