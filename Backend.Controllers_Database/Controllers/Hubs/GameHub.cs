using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.DTOs;
using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.IRepository;
using Microsoft.AspNetCore.SignalR;
using Sprache;
using WiseBet.backend.Data;
using Microsoft.VisualBasic;
using WiseBet.backend.Services;
namespace WiseBet.backend.Hubs;

public class GameHub : Hub
{
    private ICoinflipService _coinflip;
    public GameHub(ICoinflipService coinflip)
    {
        _coinflip = coinflip;
    }

    public async Task PlayRound(Guid UserId, int Amount, CoinSide ChosenSide)
    {
        var result = await _coinflip.PlayRound(UserId, Amount, ChosenSide);
        
        if (result.Fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", result.Message);
            // this.Context.User.AddIdentity
            return;
        }
        await Clients.Caller.SendAsync("UpdateClient", result);
    }
    

}