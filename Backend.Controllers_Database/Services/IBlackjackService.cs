using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.Data;
using System.ComponentModel.DataAnnotations;
namespace WiseBet.backend.Services;

public interface IBlackjackService
{
    Task<GameState> StartRound(Guid id, int bet);
    Task<GameState> Hit(Guid id);
    Task<GameState> Stand(Guid id);
}