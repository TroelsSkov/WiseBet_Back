using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.Data;
using WiseBet.backend.Security;
using WiseBet.backend.Controllers.DTOs;
using Microsoft.AspNetCore.Authorization;
namespace WiseBet.backend.Controllers
{
    //Https:localhost:5277/Api/UserAccounts
    [Route("Api/Users/[Controller]")]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

    [Authorize]
    public class AuthController : ControllerBase
    {
        // private readonly SecurityDbContext _context;
        private readonly SecurityService _secService;

        public AuthController(SecurityService secService)
        {
            _secService = secService;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUser([FromBody] AuthRegisterDto reg)
        {
            var result = await _secService.ValidateRegisterRequest(reg);
            if (!result) return BadRequest("Registration failed");

            return Ok();
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser([FromBody] AuthLoginDto login)
        {
            var result = await _secService.ValidateLoginRequest(login);
            if (!result) return BadRequest("Login failed");

            return Ok();
        }

        [HttpGet]
        public IActionResult AuthTest()
        {
            return Ok($"Du er authed: {User.FindFirst("UserRepoConnect")?.Value}");
        }
    }


}