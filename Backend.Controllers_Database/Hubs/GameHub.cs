using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.DTOs;
using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.IRepository;
using Microsoft.AspNetCore.SignalR;
using Sprache;
using WiseBet.backend.Data;
using Microsoft.VisualBasic;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.Services.Coinflip;
using WiseBet.backend.Services.Coinflip.Validation;
using WiseBet.backend.Services.Roulette;
using WiseBet.backend.Services.DTOs;

namespace WiseBet.backend.Hubs;


public class GameHub : Hub
{
    private static readonly Dictionary<string, Guid> RouletteConnections = new();
    private static readonly object RouletteConnectionsLock = new();

    private ICoinflipService _coinflip;
    private IGeneralValidation _validate;
    private IBlackjackService _blackjack;
    private IRouletteService _roulette;

    public GameHub(ICoinflipService coinflip, IGeneralValidation validation, IBlackjackService blackjack, IRouletteService roulette)
    {
        _coinflip = coinflip;
        _validate = validation;
        _blackjack = blackjack;
        _roulette = roulette; 
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

    public async Task StartRoundBlackjack(Guid UserId, int bet)
    {
        Console.WriteLine($"[GameHub] PLayer information:\n   UserID: {UserId}\n   Amount: {bet}\n");
        var validate = await _validate.ValidateBet(UserId, bet);

        if (validate.Fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }
        try
        {
            var result = await _blackjack.StartRound(UserId, bet);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }
    public async Task HitBlackjack(Guid UserId)
    {
        try
        {
            var result = await _blackjack.Hit(UserId);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }
    public async Task StandBlackjack(Guid UserId)
    {
        try
        {
            var result = await _blackjack.Stand(UserId);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public async Task JoinRouletteSession(Guid UserId)
    {
        try
        {
            var result = await _roulette.JoinRouletteSession(UserId);
            var group = result.SessionId.ToString();
            lock (RouletteConnectionsLock)
            {
                RouletteConnections[Context.ConnectionId] = UserId;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await Clients.Group(group).SendAsync("RouletteUpdated", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public async Task LeaveRouletteSession(Guid UserId)
    {
        try
        {
            var result = await _roulette.LeaveRouletteSession(UserId);
            var group = result.SessionId.ToString();
            lock (RouletteConnectionsLock)
            {
                RouletteConnections.Remove(Context.ConnectionId);
            }
            await Clients.Group(group).SendAsync("RouletteUpdated", result);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public async Task PlaceRouletteBet(Guid UserId, RouletteBetDto bet)
    {
        if (bet == null)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Bet payload is required.");
            return;
        }

        var validate = await _validate.ValidateBet(UserId, bet.Amount);
        if (validate.Fail)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }

        try
        {
            var result = await _roulette.PlaceRouletteBet(UserId, bet);
            await Clients.Group(result.SessionId.ToString()).SendAsync("RouletteUpdated", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid userId;
        lock (RouletteConnectionsLock)
        {
            if (!RouletteConnections.TryGetValue(Context.ConnectionId, out userId))
            {
                return;
            }
            RouletteConnections.Remove(Context.ConnectionId);
        }

        try
        {
            var result = await _roulette.LeaveRouletteSession(userId);
            await Clients.Group(result.SessionId.ToString()).SendAsync("RouletteUpdated", result);
        }
        catch
        {
            // Best effort cleanup when a disconnected user is no longer in session.
        }
        finally
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}