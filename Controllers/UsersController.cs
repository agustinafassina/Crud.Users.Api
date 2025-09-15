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

        [HttpGet("pet-one")]
        public async Task<IActionResult> GetVersion()
        {
            return Ok("Cleooo :!");
        }

        [HttpGet("pet-two")]
        public IActionResult GetItems()
        {
            IEnumerable<Services.Dto.ItemDto>? items = _sessionService.GetAllItems();
            return Ok(items);
        }

        [HttpGet("pet-three/{id}")]
        public IActionResult GetById(int id)
        {
            Services.Dto.ItemDto item = _sessionService.GetItemById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateSession([FromBody] SessionCreateDto dto)
        {
            await _sessionService.CreateSessionAsync(dto.UserId, dto.SessionData);
            return Ok(new { message = "Session creada" });
        }
    }
}