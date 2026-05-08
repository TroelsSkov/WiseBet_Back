using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.Data;
using WiseBet.backend.Security;
using WiseBet.backend.Controllers.DTOs;
using Microsoft.AspNetCore.Authorization;
using WiseBet.backend.IRepository;
using System.Runtime.CompilerServices;
namespace WiseBet.backend.Controllers
{
    //Https:localhost:5277/Api/UserAccounts
    [Route("Api/Users/me/[Controller]")]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

    [Authorize]
    public class UserAccountController : ControllerBase
    {

        private readonly UserAccountRepository _userRepo;
        public UserAccountController(UserAccountRepository repo)
        {
            _userRepo = repo;
        }

        [HttpGet("saldo")]
        public async Task<IActionResult> GetUserBalance()
        {
            try
            {
                Guid.TryParse(this.User.FindFirst("UserRepoConnect")?.Value, out Guid userId);
                var user = await _userRepo.GetByIdAsync(userId);
                return Ok(user.Saldo);
            }
            catch (SystemException e)
            {
                Console.WriteLine($"[UserAccountController] Unable to get user balance: {e.Message}");
                return BadRequest();
            }
        }
        [HttpGet("username")]
        public async Task<IActionResult> GetUsername()
        {
            try
            {
                Guid.TryParse(this.User.FindFirst("UserRepoConnect")?.Value, out Guid userId);
                var user = await _userRepo.GetByIdAsync(userId);
                return Ok(user.Username);
            }
            catch (SystemException e)
            {
                Console.WriteLine($"[UserAccountController] Unable to get username: {e.Message}");
                return BadRequest();
            }
        }
    }


}