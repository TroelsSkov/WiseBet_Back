using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.Data;
using WiseBet.backend.Models;
namespace WiseBet.backend.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class BaseController : ControllerBase
   {
      private readonly DatabaseContext context;
      public BaseController(DatabaseContext db)
      {
         context = db;
      }
      [HttpGet]
      public IActionResult Get()
      {
         return BadRequest("API not defined");
      }

      [HttpDelete("{id}")]
      public IActionResult Delete(int id)
      {
         //Logic til at delete

         return NoContent();
      }
   }
}
