using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Data;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.IRepository;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace WiseBet.backend.Services;

public class BlackjackService : IBlackjackService
{
    private Dictionary<Guid, GameState> _activeGames = new Dictionary<Guid, GameState>();
    private readonly UserAccountRepository _userRepo;
    private readonly Func<IDeck> _deckFactory;

    public BlackjackService(UserAccountRepository userRepo, Func<IDeck> deckFactory)
    {
        _userRepo = userRepo;
        _deckFactory = deckFactory;
    }

    private int CalculateScoreDealer(Card card)
    {
        return card.Value;
    }
    private int CalculateScore(List<Card> hand)
    {
        var result = 0;
        int aces = 0;
        foreach (Card card in hand)
        {
            result += card.Value;
            if (card.Rank == Rank.Ace)
                aces++;
        }
        while (result > 21 && aces > 0)
        {
            result -= 10;
            aces--;
        }
        return result;
    }

    private BlackjackDto BuildDto(GameState gameState)
    {
        return new BlackjackDto
        {
            PlayerHand = gameState.PlayerHand,
            DealerVisibleHand = gameState.State == GameStatus.Playing
                ? new List<Card> { gameState.DealerHand[0] }
                : gameState.DealerHand,
            Status = gameState.State,
            PlayerScore = CalculateScore(gameState.PlayerHand),
            DealerScore = gameState.State == GameStatus.Playing
            ? CalculateScoreDealer(gameState.DealerHand[0])
            : CalculateScore(gameState.DealerHand)
        };
    }

    public async Task PlayBJRound(ISingleClientProxy caller, CancellationToken cancellationToken, Guid userID, int bet)
    {
        Console.WriteLine($"[BlackJackService] Has started!");

        var dto = await StartRound(userID, bet);
        await caller.SendAsync("UpdateClient", dto);
        var nextAction = await caller.InvokeAsync<string>("NextAction", cancellationToken);
        Console.WriteLine($"[BlackJackService] We saw the follwing from the user: {nextAction}");
        // while (true)
        // {
            nextAction = "";
            nextAction = await caller.InvokeAsync<string>("NextAction", cancellationToken);
            if (nextAction == "hit")
            {
                dto = await Hit(userID);
                await caller.SendAsync("UpdateClient", dto);
            }
            else if (nextAction == "stand")
            {
                dto = await Stand(userID);
                await caller.SendAsync("UpdateClient", dto);
            }
        // }
    }


    public async Task<BlackjackDto> StartRound(Guid id, int bet)
    {
        var user = await _userRepo.GetByIdAsync(id);
        user.Saldo -= bet;
        await _userRepo.PutAsync(id, user);

        var gameState = new GameState(_deckFactory());
        _activeGames[id] = gameState;
        gameState.Bet = bet;
        gameState.State = GameStatus.Playing;
        gameState.PlayerHand.Add(gameState.Deck.draw());
        gameState.DealerHand.Add(gameState.Deck.draw());
        gameState.PlayerHand.Add(gameState.Deck.draw());
        gameState.DealerHand.Add(gameState.Deck.draw());

        if (CalculateScore(gameState.DealerHand) == 21 && CalculateScore(gameState.PlayerHand) == 21)
        {
            gameState.State = GameStatus.Push;
            user.Saldo += bet;
            await _userRepo.PutAsync(id, user);
        }
        else if (CalculateScore(gameState.PlayerHand) == 21)
        {
            gameState.State = GameStatus.PlayerWin;
            user.Saldo += (int)(bet * 2.5);
            await _userRepo.PutAsync(id, user);
        }

        if (gameState.State != GameStatus.Playing)
            _activeGames.Remove(id);

        return BuildDto(gameState);
    }

    public async Task<BlackjackDto> Hit(Guid id)
    {
        var gameState = _activeGames[id];
        gameState.PlayerHand.Add(gameState.Deck.draw());

        if (CalculateScore(gameState.PlayerHand) > 21)
        {
            gameState.State = GameStatus.PlayerBust;
            _activeGames.Remove(id);
        }

        return BuildDto(gameState);
    }

    public async Task<BlackjackDto> Stand(Guid id)
    {
        var user = await _userRepo.GetByIdAsync(id);

        var gameState = _activeGames[id];
        if (CalculateScore(gameState.DealerHand) == 21)
        {
            gameState.State = GameStatus.DealerWin;
            _activeGames.Remove(id);
            return BuildDto(gameState);
        }


        while (CalculateScore(gameState.DealerHand) < 17)
        {
            gameState.DealerHand.Add(gameState.Deck.draw());
            if (CalculateScore(gameState.DealerHand) > 21)
            {
                gameState.State = GameStatus.DealerBust;
                user.Saldo += gameState.Bet * 2;
                await _userRepo.PutAsync(id, user);
                break;
            }
        }

        if (gameState.State != GameStatus.DealerBust)
        {
            if (CalculateScore(gameState.PlayerHand) > CalculateScore(gameState.DealerHand))
            {
                gameState.State = GameStatus.PlayerWin;
                user.Saldo += gameState.Bet * 2;
                await _userRepo.PutAsync(id, user);
            }
            else if (CalculateScore(gameState.PlayerHand) < CalculateScore(gameState.DealerHand))
                gameState.State = GameStatus.DealerWin;
            else
            {
                gameState.State = GameStatus.Push;
                user.Saldo += gameState.Bet;
                await _userRepo.PutAsync(id, user);
            }
        }
        else if (CalculateScore(gameState.DealerHand) == 21)
            gameState.State = GameStatus.DealerWin;

        _activeGames.Remove(id);
        return BuildDto(gameState);
    }
}