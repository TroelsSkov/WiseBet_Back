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
    private IBlackjackService _blackjack;
    public GameHub(ICoinflipService coinflip, IGeneralValidation validation, IBlackjackService blackjack)
    {
        _coinflip = coinflip;
        _validate = validation;
        _blackjack = blackjack;
    }

        public async Task PlayRound(int Amount, CoinSide ChosenSide)
    {
        var UserIdString = this.Context.User?.FindFirst("UserRepoConnect")?.Value;
        Guid.TryParse(UserIdString, out Guid UserId);
        Console.WriteLine($"[GameHub] PLayer information:\n   UserID: {UserIdString}\n   Amount: {Amount}\n   Chosenside: {ChosenSide}");
        Console.WriteLine($"[GameHub] PLayer information:\n   UserID: {UserId}\n   Amount: {Amount}\n   Chosenside: {ChosenSide}");

        var validate = await _validate.ValidateBet(UserId, Amount);

        if (validate.Fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }
        await _coinflip.PlayRound(UserId, Amount, ChosenSide);
    }
    public async Task StartRoundBlackjack( int bet)
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
            CancellationToken cancellationToken = this.Context.ConnectionAborted;
            await _blackjack.PlayBJRound(this.Clients.Caller, cancellationToken, userId, bet);
            Console.WriteLine("[GameHub] The blackjack round concluded...");
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }
    // public async Task HitBlackjack()
    // {
    //     var userClaim = this.Context.User?.FindFirst("UserRepoConnect")?.Value;
    //     Guid.TryParse(userClaim, out var userId);
    //     try
    //     {
    //         var result = await _blackjack.Hit(userId);
    //         await Clients.Caller.SendAsync("UpdateClient", result);
    //     }
    //     catch (Exception e)
    //     {
    //         await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
    //     }
    // }
    // public async Task StandBlackjack()
    // {
    //     var userClaim = this.Context.User?.FindFirst("UserRepoConnect")?.Value;
    //     Guid.TryParse(userClaim, out var userId);
    //     try
    //     {
    //         var result = await _blackjack.Stand(userId);
    //         await Clients.Caller.SendAsync("UpdateClient", result);
    //     }
    //     catch (Exception e)
    //     {
    //         await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
    //     }
    // }

    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"[Hub] Client connected: {this.Context.User?.FindFirst("UserRepoConnect")?.Value}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"[GameHub] Player {this.Context.User?.FindFirst("UserRepoConnect")?.Value} has disconnected");
        await base.OnDisconnectedAsync(exception);
    }
}