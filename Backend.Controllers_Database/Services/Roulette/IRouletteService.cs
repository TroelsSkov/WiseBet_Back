using WiseBet.backend.Services.DTOs;

namespace WiseBet.backend.Services.Roulette;

public interface IRouletteService
{
    Task<RouletteDto> JoinRouletteSession(Guid userId);
    Task<RouletteDto> LeaveRouletteSession(Guid userId);
    Task<RouletteDto> PlaceRouletteBet(Guid userId, RouletteBetDto bet);
}