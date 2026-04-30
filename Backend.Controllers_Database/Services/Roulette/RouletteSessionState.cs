using WiseBet.backend.Data;
using WiseBet.backend.Services.DTOs;

namespace WiseBet.backend.Services.Roulette;

public class RouletteSessionState
{
    public Guid SessionId { get; set; } = Guid.NewGuid();
    public RouletteSessionStatus Status { get; set; } = RouletteSessionStatus.WaitingForPlayers;
    public HashSet<Guid> Participants { get; set; } = new();
    public DateTime RoundStartedUtc { get; set; } = DateTime.UtcNow;
    public int RoundDurationSeconds { get; set; } = 30;

    public List<(Guid UserId, RouletteBetDto Bet)> Bets { get; set; } = new();
}