using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.Data;
using WiseBet.backend.Controllers.DTOs;
namespace WiseBet.backend.Controllers{
//Https:localhost:5277/Api/UserAccounts
    [Route("Api/Games/[Controller]")]
    [ApiController]
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class CoinFlipController
    {
        public CoinFlipController(DatabaseContext DbContext)
        {
            
        }
        
        [HttpGet("PlayRound")]
        public async Task<CoinFlipDTO> PlayRound()
        {
            throw new NotImplementedException(); 
        }
        
    }

}