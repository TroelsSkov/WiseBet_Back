using Microsoft.AspNetCore.Mvc;
using WiseBet.backend.DTOs;
using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.IRepository;
using Microsoft.AspNetCore.SignalR;
using Sprache;
using WiseBet.backend.Data;
using WiseBet.backend.Controllers;
namespace WiseBet.backend.Services;

public interface ICoinflipService
{
    Task<CoinFlipDTO> PlayRound(Guid userId, int amount, CoinSide chosenSide);
}