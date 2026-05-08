using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.Data;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.SignalR;
namespace WiseBet.backend.Services.Blackjack;

public interface IBlackjackService
{
    Task PlayBJRound(ISingleClientProxy caller, CancellationToken cancellationToken, Guid userId, int bet);
    Task<BlackjackDto> StartRound(Guid id, int bet);
    Task<BlackjackDto> Hit(Guid id);
    Task<BlackjackDto> Stand(Guid id);
}