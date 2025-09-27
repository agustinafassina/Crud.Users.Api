using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace UsersApi.Controllers
{
    [ApiController]
    [Route("api/v2/[controller]")]
    public class SecurityReview : ControllerBase
    {

        public SecurityReview()
        {
        }

        [Authorize(AuthenticationSchemes = "SecurityAuth")]
        [HttpGet("test-one")]
        public async Task<IActionResult> GetMethodReviewOne()
        {
            return Ok("1.0.0");
        }

        [Authorize(AuthenticationSchemes = "SecurityAuth")]
        [HttpPost("test-two")]
        public async Task<IActionResult> PostMethodReviewTestTwo()
        {
            return Ok("It´s ok!");
        }

        [HttpGet("without-auth")]
        public async Task<IActionResult> GetMethodReviewTestThree()
        {
            return Ok("It´s ok too!");
        }
    }
}