using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.Data;
using WiseBet.backend.Models;
namespace WiseBet.backend.Controllers{
//Https:localhost:5277/Api/UserAccounts
    [Route("Api/[Controller]")]
    [ApiController]
    public class UserAccountController: ControllerBase
    {
        private readonly DatabaseContext DbContext;

        public UserAccountController(DatabaseContext DbContext)
        {
            this.DbContext = DbContext;
        }

        [HttpGet]
        public IActionResult GetAllUsers()
        {
            var Accounts = DbContext.UserAccounts.ToList();
            return Ok(Accounts);
        }

        [HttpPost]
        public IActionResult CreateUser(UserAccount account)
      {
        DbContext.UserAccounts.Add(account);
        DbContext.SaveChanges();
        return Ok(account);
      }
        
    }




}