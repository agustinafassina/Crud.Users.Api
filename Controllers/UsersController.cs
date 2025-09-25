using AutoMapper;
using UsersApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Mappers.Requests;
using UsersApi.Services.Dto;
using Microsoft.AspNetCore.Authorization;

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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            UserEntity userEntity = _mapper.Map<UserEntity>(dto);
            var user = await _userService.CreateUserAsync(userEntity);

            // Fatla cambiar el return por CreatedAtAction
            return Ok(user);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            List<UserEntity>? userEntities = await _userService.GetUsers();
            return Ok(userEntities);
        }

        [HttpPost("generate-token")]
        public async Task<IActionResult> GenerateToken([FromBody] LoginRequest loginRequest)
        {
            var user = await _userService.ValidateUserAsync(loginRequest.Email, loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("The user does not exist");
            }

            string tokenString = _userService.GenerateJwtToken(user);
            return Ok(tokenString);
        }


        [Authorize(AuthenticationSchemes = "SecurityAuth")]
        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            return Ok("1.0.0");
        }

    }
}