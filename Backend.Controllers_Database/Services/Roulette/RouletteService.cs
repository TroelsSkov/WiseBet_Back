using Microsoft.AspNetCore.SignalR;
using WiseBet.backend.Data;
using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Services.Roulette;

namespace WiseBet.backend.Services.Roulette;

public class RouletteService : IRouletteService
{
    private readonly List<RouletteSessionState> _sessions = new();
    private const int MaxUsersPerSession = 5;

    public Task<RouletteDto> JoinRouletteSession(Guid userId)
    {
        // Hvis user allerede er i en session, returner den
        var existingSession = _sessions.FirstOrDefault(s => s.Participants.Contains(userId));
        if (existingSession != null)
            return Task.FromResult(BuildDto(existingSession, null, null));

        // Find en session med plads
        var session = _sessions.FirstOrDefault(s => s.Participants.Count < MaxUsersPerSession);

        // Hvis ingen session har plads, opret ny
        if (session == null)
        {
            session = new RouletteSessionState
            {
                SessionId = Guid.NewGuid(),
                Status = RouletteSessionStatus.BettingOpen,
                RoundStartedUtc = DateTime.UtcNow,
                RoundDurationSeconds = 30
            };
            _sessions.Add(session);
        }

        session.Participants.Add(userId);

        return Task.FromResult(BuildDto(session, null, null));
    }

    public Task<RouletteDto> LeaveRouletteSession(Guid userId)
    {
        var session = _sessions.FirstOrDefault(s => s.Participants.Contains(userId));
        if (session == null)
            throw new KeyNotFoundException("User is not in any roulette session.");

        session.Participants.Remove(userId);

        // Fjern brugerens åbne bets i sessionen
        session.Bets.RemoveAll(b => b.UserId == userId);

        // Cleanup tomme sessions
        if (session.Participants.Count == 0)
            _sessions.Remove(session);

        return Task.FromResult(BuildDto(session, null, null));
    }

    public Task<RouletteDto> PlaceRouletteBet(/*this ISingleClientProxy client,*/ Guid userId, RouletteBetDto bet)
    {
        var session = _sessions.FirstOrDefault(s => s.Participants.Contains(userId));
        if (session == null)
            throw new KeyNotFoundException("User is not in any roulette session.");

        if (bet == null)
            throw new ArgumentNullException(nameof(bet));

        if (bet.Amount <= 0)
            throw new ArgumentException("Bet amount must be greater than zero.");

        RefreshRoundWindow(session);

        if (session.Status != RouletteSessionStatus.BettingOpen)
            throw new InvalidOperationException("Betting is closed for this round.");

        session.Bets.Add((userId, bet));

        return Task.FromResult(BuildDto(session, null, null));
    }

    private static void RefreshRoundWindow(RouletteSessionState session)
    {
        var elapsed = (DateTime.UtcNow - session.RoundStartedUtc).TotalSeconds;

        if (elapsed <= session.RoundDurationSeconds)
        {
            session.Status = RouletteSessionStatus.BettingOpen;
            return;
        }

        // Enkel v1: runde afsluttes og ny runde starter straks
        session.Status = RouletteSessionStatus.RoundFinished;
        session.Bets.Clear();
        session.RoundStartedUtc = DateTime.UtcNow;
        session.Status = RouletteSessionStatus.BettingOpen;
    }

    private static RouletteDto BuildDto(
        RouletteSessionState session,
        int? winningNumber,
        RouletteBetType? winningColor)
    {
        var secondsLeft = session.RoundDurationSeconds -
                          (int)(DateTime.UtcNow - session.RoundStartedUtc).TotalSeconds;

        if (secondsLeft < 0) secondsLeft = 0;
        if (secondsLeft > session.RoundDurationSeconds) secondsLeft = session.RoundDurationSeconds;

        return new RouletteDto
        {
            SessionId = session.SessionId,
            Status = session.Status,
            ActiveUsers = session.Participants.Count,
            MaxUsers = MaxUsersPerSession,
            SecondsLeft = secondsLeft,
            WinningNumber = winningNumber,
            WinningColor = winningColor,
            Participants = session.Participants.ToList()
        };
    }
}