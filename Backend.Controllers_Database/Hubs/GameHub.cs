using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using WiseBet.backend.Data;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.Services.Coinflip;
using WiseBet.backend.Services.Coinflip.Validation;
using WiseBet.backend.Services.Roulette;
using WiseBet.backend.Services.DTOs;

namespace WiseBet.backend.Hubs;

[Authorize]
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

    private bool TryGetAuthenticatedUserId(out Guid userId)
    {
        userId = default;
        var principal = Context.User;
        if (principal == null)
            return false;

        var candidate = principal.FindFirst("UserRepoConnect")?.Value
            ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        return !string.IsNullOrWhiteSpace(candidate) && Guid.TryParse(candidate, out userId);
    }

    public async Task PlayRound(int Amount, CoinSide ChosenSide)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Kunne ikke finde bruger. Log ind igen.");
            return;
        }

        var validate = await _validate.ValidateBet(userId, Amount);

        if (validate.Fail == true)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }
        try
        {
            var result = await _coinflip.PlayRound(userId, Amount, ChosenSide);
            await Clients.Caller.SendAsync("UpdateClient", result);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public async Task StartRoundBlackjack(int bet)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Kunne ikke finde bruger. Log ind igen.");
            return;
        }

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
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Kunne ikke finde bruger. Log ind igen.");
            return;
        }

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
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Kunne ikke finde bruger. Log ind igen.");
            return;
        }

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

    private async Task BroadcastRouletteSessionAsync(RouletteSessionUpdate update)
    {
        foreach (var dto in update.BroadcastFirst)
        {
            await Clients.Group(dto.SessionId.ToString()).SendAsync("UpdateClient", dto);
        }

        await Clients.Group(update.Current.SessionId.ToString()).SendAsync("UpdateClient", update.Current);
    }

    public async Task JoinRouletteSession()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Kunne ikke finde bruger. Log ind igen.");
            return;
        }

        try
        {
            var update = await _roulette.JoinRouletteSession(userId);
            var group = update.Current.SessionId.ToString();
            lock (RouletteConnectionsLock)
            {
                RouletteConnections[Context.ConnectionId] = userId;
            }
            await Groups.AddToGroupAsync(Context.ConnectionId, group);
            await BroadcastRouletteSessionAsync(update);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public async Task LeaveRouletteSession()
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Kunne ikke finde bruger. Log ind igen.");
            return;
        }

        try
        {
            var update = await _roulette.LeaveRouletteSession(userId);
            var group = update.Current.SessionId.ToString();
            lock (RouletteConnectionsLock)
            {
                RouletteConnections.Remove(Context.ConnectionId);
            }
            await BroadcastRouletteSessionAsync(update);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public async Task PlaceRouletteBet(RouletteBetDto bet)
    {
        if (!TryGetAuthenticatedUserId(out var userId))
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Kunne ikke finde bruger. Log ind igen.");
            return;
        }

        if (bet == null)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", "Bet payload is required.");
            return;
        }

        var validate = await _validate.ValidateBet(userId, bet.Amount);
        if (validate.Fail)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", validate.Message);
            return;
        }

        try
        {
            var update = await _roulette.PlaceRouletteBet(userId, bet);
            await BroadcastRouletteSessionAsync(update);
        }
        catch (Exception e)
        {
            await Clients.Caller.SendAsync("ErrorMessageToClient", e.Message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid userId;
        var hadRoulette = false;
        lock (RouletteConnectionsLock)
        {
            hadRoulette = RouletteConnections.TryGetValue(Context.ConnectionId, out userId);
            if (hadRoulette)
                RouletteConnections.Remove(Context.ConnectionId);
        }

        if (hadRoulette)
        {
            try
            {
                var update = await _roulette.LeaveRouletteSession(userId);
                await BroadcastRouletteSessionAsync(update);
            }
            catch
            {
                // Best effort cleanup when a disconnected user is no longer in session.
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
