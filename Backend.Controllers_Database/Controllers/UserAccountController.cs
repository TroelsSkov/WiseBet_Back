using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.Models;
namespace WiseBet.backend.Controllers{
//Https:localhost:5277/Api/UserAccounts
    [Route("Api/[Controller]")]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UserAccountController : BaseController<UserAccountDto>
    {
        public UserAccountController(DatabaseContext DbContext) : base(new UserAccountRepository(DbContext))
        {
            
        }
        [HttpGet]
        public async Task<ActionResult<List<UserAccountDto>>> GetUserAccount()
        {
            var users = await repository.GetAllAsync();
            return Ok(users);
        }
    }

}