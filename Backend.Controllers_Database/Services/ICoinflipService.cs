using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Data;
namespace WiseBet.backend.Services;

public interface ICoinflipService
{
    Task<CoinFlipDTO> PlayRound(Guid userId, int amount, CoinSide chosenSide);
}