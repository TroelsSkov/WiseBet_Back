using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.Services.DTOs;

namespace WiseBet.backend.Services.Roulette;

public class RouletteService : IRouletteService
{
    private readonly UserAccountRepository _userRepo;
    private readonly RoundRepository _roundRepo;
    private readonly BetRepository _betRepo;

    private readonly List<RouletteSessionState> _sessions = new();
    private const int MaxUsersPerSession = 5;

    public RouletteService(
        UserAccountRepository userRepo,
        RoundRepository roundRepo,
        BetRepository betRepo)
    {
        _userRepo = userRepo;
        _roundRepo = roundRepo;
        _betRepo = betRepo;
    }

    public async Task<RouletteDto> JoinRouletteSession(Guid userId)
    {
        var existing = _sessions.FirstOrDefault(s => s.Participants.Contains(userId));
        if (existing != null)
            return BuildDto(existing);

        var session = _sessions.FirstOrDefault(s => s.Participants.Count < MaxUsersPerSession);
        if (session == null)
        {
            session = new RouletteSessionState
            {
                Status = RouletteSessionStatus.BettingOpen,
                RoundStartedUtc = DateTime.UtcNow
            };
            _sessions.Add(session);

            // Opret DB round når ny session starter sin round
            var roundDto = new RoundDto
            {
                ID = Guid.NewGuid(),
                RoundPlayDate = DateTime.UtcNow
            };
            await _roundRepo.PostAsync(roundDto);
            session.CurrentRoundId = roundDto.ID;
        }

        session.Participants.Add(userId);
        return BuildDto(session);
    }

    public Task<RouletteDto> LeaveRouletteSession(Guid userId)
    {
        var session = _sessions.FirstOrDefault(s => s.Participants.Contains(userId))
            ?? throw new System.Collections.Generic.KeyNotFoundException("User is not in any roulette session.");;

        session.Participants.Remove(userId);
        session.Bets.RemoveAll(x => x.UserId == userId);

        if (session.Participants.Count == 0)
            _sessions.Remove(session);

        return Task.FromResult(BuildDto(session));
    }

    public async Task<RouletteDto> PlaceRouletteBet(Guid userId, RouletteBetDto bet)
    {
        var session = _sessions.FirstOrDefault(s => s.Participants.Contains(userId))
            ?? throw new System.Collections.Generic.KeyNotFoundException("User is not in any roulette session.");

        if (bet.Amount <= 0)
            throw new ArgumentException("Bet amount must be greater than zero.");

        RefreshRoundWindow(session);

        if (session.Status != RouletteSessionStatus.BettingOpen)
            throw new InvalidOperationException("Betting is closed for this round.");

        var user = await _userRepo.GetByIdAsync(userId);
        if (user.Saldo < bet.Amount)
            throw new InvalidOperationException("You cant afford this bet");

        // Træk saldo ved bet placement
        user.Saldo -= bet.Amount;
        await _userRepo.PutAsync(userId, user);

        session.Bets.Add((userId, bet));

        // Persist bet
        if (session.CurrentRoundId == null)
            throw new InvalidOperationException("Round not initialized.");

        var betDto = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = userId,
            RoundId = session.CurrentRoundId.Value,
            Amount = bet.Amount,

            // Map til jeres outcomes når I har faste outcome IDs
            OutcomeID = 0,
            OutcomeDescription = bet.BetType.ToString()
        };
        await _betRepo.PostAsync(betDto);

        return BuildDto(session);
    }

    private async Task ResolveRoundIfExpired(RouletteSessionState session)
    {
        var elapsed = (DateTime.UtcNow - session.RoundStartedUtc).TotalSeconds;
        if (elapsed <= session.RoundDurationSeconds) return;

        session.Status = RouletteSessionStatus.Spinning;

        var winningNumber = Random.Shared.Next(0, 37);
        var winningColor = MapNumberToColor(winningNumber);

        session.WinningNumber = winningNumber;
        session.WinningColor = winningColor;

        // payout
        foreach (var (userId, bet) in session.Bets)
        {
            var isWin = bet.BetType == winningColor;
            if (!isWin) continue;

            var payout = bet.BetType == RouletteBetType.Green
                ? bet.Amount * 14
                : bet.Amount * 2;

            var user = await _userRepo.GetByIdAsync(userId);
            user.Saldo += payout;
            await _userRepo.PutAsync(userId, user);
        }

        session.Status = RouletteSessionStatus.RoundFinished;

        // TODO: update RoundRepository med outcome/payout/earnings

        // ny round
        session.Bets.Clear();
        session.RoundStartedUtc = DateTime.UtcNow;
        session.Status = RouletteSessionStatus.BettingOpen;
    }

    private void RefreshRoundWindow(RouletteSessionState session)
    {
        var elapsed = (DateTime.UtcNow - session.RoundStartedUtc).TotalSeconds;
        if (elapsed <= session.RoundDurationSeconds)
            session.Status = RouletteSessionStatus.BettingOpen;
    }

    private static RouletteBetType MapNumberToColor(int n)
    {
        if (n == 0) return RouletteBetType.Green;
        return n % 2 == 0 ? RouletteBetType.Black : RouletteBetType.Red;
    }

    private static RouletteDto BuildDto(RouletteSessionState s) => new()
    {
        SessionId = s.SessionId,
        Status = s.Status,
        ActiveUsers = s.Participants.Count,
        MaxUsers = MaxUsersPerSession,
        SecondsLeft = Math.Max(0, s.RoundDurationSeconds - (int)(DateTime.UtcNow - s.RoundStartedUtc).TotalSeconds),
        WinningNumber = s.WinningNumber,
        WinningColor = s.WinningColor,
        Participants = s.Participants.ToList()
    };
}