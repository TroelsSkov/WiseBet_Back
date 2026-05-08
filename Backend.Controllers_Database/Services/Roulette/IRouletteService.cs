using WiseBet.backend.Services.DTOs;

namespace WiseBet.backend.Services.Roulette;

public interface IRouletteService
{
    Task<RouletteSessionUpdate> JoinRouletteSession(Guid userId);
    Task<RouletteSessionUpdate> LeaveRouletteSession(Guid userId);
    Task<RouletteSessionUpdate> PlaceRouletteBet(Guid userId, RouletteBetDto bet);

  
    // Køres ca. hvert sekund: afvikler udløbne runder og returnerer DTO’er til broadcast (færdig runde + ny state).
    Task<IReadOnlyList<RouletteDto>> ProcessSessionTimersAsync();
}
