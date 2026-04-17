using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.DTOs;
using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.IRepository;
using Microsoft.AspNetCore.SignalR;
using Sprache;
using WiseBet.backend.Data;
using Microsoft.VisualBasic;
using WiseBet.backend.Services;
using Microsoft.AspNetCore.Authorization;
namespace WiseBet.backend.Hubs;

[Authorize]
public class GameHub : Hub
{
    private ICoinflipService _coinflip;
    private IGeneralValidation _validate;
    public GameHub(ICoinflipService coinflip, IGeneralValidation validation)
    {
        _coinflip = coinflip;
        _validate = validation;
    }

    public async Task PlayRound(Guid UserId, int Amount, CoinSide ChosenSide)
    {

        Console.WriteLine($"[Gamehub] Sent infomration: {this.Context.User?.FindFirst("UserRepoConnect")?.Value}");
        Console.WriteLine($"[Gamehub] Authentication status: {this.Context.User?.Identity?.IsAuthenticated}");
        Console.WriteLine($"[GameHub] PLayer information:\n   UserID: {UserId}\n   Amount: {Amount}\n   Chosenside: {ChosenSide}");
        UserId = Guid.Parse(this.Context.User?.FindFirst("UserRepoConnect")?.Value);
        var validate = await _validate.ValidateBet(UserId, Amount);

        if (validate.Fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }

        var result = await _coinflip.PlayRound(UserId, Amount, ChosenSide);

        await Clients.Caller.SendAsync("UpdateClient", result);
    }
}