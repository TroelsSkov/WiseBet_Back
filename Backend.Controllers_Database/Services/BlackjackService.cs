using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Data;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.IRepository;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace WiseBet.backend.Services;

public class BlackjackService : IBlackjackService
{
    private Dictionary<Guid, GameState> _activeGames = new Dictionary<Guid, GameState>();
    private readonly UserAccountRepository _userRepo;
    private int CalculateScore(List<Card> hand)
    {
        var result = 0;
        foreach(Card card in hand)
        {
            result += card.Value;
        }
        return result;
    }

    public BlackjackService(UserAccountRepository userRepo)
    {
        _userRepo = userRepo;
    }
    public async Task<BlackjackDto> StartRound(Guid id, int bet)
    {
        var gameState = new GameState();
        _activeGames[id] = gameState;
        gameState.Bet = bet;
        gameState.State = GameStatus.Playing;
        gameState.PlayerHand.Add(gameState.Deck.draw());
        gameState.DealerHand.Add(gameState.Deck.draw());
        gameState.PlayerHand.Add(gameState.Deck.draw());
        gameState.DealerHand.Add(gameState.Deck.draw());

        if(CalculateScore(gameState.DealerHand) == 21 && CalculateScore(gameState.PlayerHand) == 21)
        {
            gameState.State = GameStatus.Push;
        }
        else if(CalculateScore(gameState.PlayerHand) == 21)
        {
            gameState.State = GameStatus.PlayerWin;
        }
        else if(CalculateScore(gameState.DealerHand) == 21)
        {
            gameState.State = GameStatus.DealerWin;
        }
        return new BlackjackDto
        {
            PlayerHand = gameState.PlayerHand,
            DealerVisibleHand = new List<Card>{gameState.DealerHand[0]},
            Status = gameState.State
        };
    }
    public async Task<BlackjackDto> Hit(Guid id)
    {
        return new BlackjackDto{};
    }
    public async Task<BlackjackDto> Stand(Guid id)
    {
        return new BlackjackDto{};
    }
}