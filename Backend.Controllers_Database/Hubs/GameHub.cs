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
    private IGeneralValidation _validate;
    private IBlackjackService _blackjack;
    public GameHub(ICoinflipService coinflip, IGeneralValidation validation, IBlackjackService blackjack)
    {
        _coinflip = coinflip;
        _validate = validation;
        _blackjack = blackjack;
    }

    public async Task PlayRound(Guid UserId, int Amount, CoinSide ChosenSide)
    {
        Console.WriteLine($"[GameHub] PLayer information:\n   UserID: {UserId}\n   Amount: {Amount}\n   Chosenside: {ChosenSide}");

        var validate = await _validate.ValidateBet(UserId, Amount);

        if (validate.Fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }
        try
        {
            var result = await _coinflip.PlayRound(UserId, Amount, ChosenSide);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public async Task StartRoundBlackjack(int bet)
    {
        var userClaim = this.Context.User.FindFirst("UserRepoConnect")?.Value;
        Guid.TryParse(userClaim, out var userId);
        Console.WriteLine($"[GameHub] PLayer information:\n   UserID: {userId}\n   Amount: {bet}\n");
        var validate = await _validate.ValidateBet(userId, bet);

        if (validate.Fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }
        try
        {
            var result = await _blackjack.StartRound(userId, bet);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }
    public async Task HitBlackjack()
    {
        var userClaim = this.Context.User.FindFirst("UserRepoConnect")?.Value;
        Guid.TryParse(userClaim, out var userId);
        try
        {
            var result = await _blackjack.Hit(userId);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }
    public async Task StandBlackjack()
    {
        var userClaim = this.Context.User.FindFirst("UserRepoConnect")?.Value;
        Guid.TryParse(userClaim, out var userId);
        try
        {
            var result = await _blackjack.Stand(userId);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }
}