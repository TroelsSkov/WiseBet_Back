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

    public async Task CoinFlip(Guid userId, int amount, CoinSide chosenSide)
    {
        var result = await _coinflip.PlayRound(userId, amount, chosenSide);
        
        if (result.fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", result.message);
            return;
        }
        await Clients.Caller.SendAsync("UpdateClient", result);
    }

}