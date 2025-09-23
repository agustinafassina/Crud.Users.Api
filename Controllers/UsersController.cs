using AutoMapper;
using UsersApi.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using UsersApi.Mappers.Requests;

namespace UsersApi.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ISessionsService _sessionService;
        private readonly IMapper _mapper;

        public UsersController(ISessionsService itemService, IMapper mapper)
        {
            _sessionService = itemService;
            _mapper = mapper;
        }

        [HttpGet("version")]
        public async Task<IActionResult> GetVersion()
        {
            return Ok("1.0.0");
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSession([FromBody] SessionCreateRequest dto)
        {
            await _sessionService.CreateSessionAsync(dto.UserId, dto.ExpirationToken, dto.Token);
            return Ok(new { message = "Session created!" });
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] SessionCreateRequest dto)
        {
            await _sessionService.CreateSessionAsync(dto.UserId, dto.ExpirationToken, dto.Token);
            return Ok(new { message = "Session created!" });
        }
    }
}