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
    private readonly RouletteSessionStore _sessionStore;
    private const int MaxUsersPerSession = 5;
    private static readonly HashSet<int> RedNumbers = new()
    {
        1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
    };

    public RouletteService(
        UserAccountRepository userRepo,
        RoundRepository roundRepo,
        BetRepository betRepo,
        RouletteSessionStore sessionStore)
    {
        _userRepo = userRepo;
        _roundRepo = roundRepo;
        _betRepo = betRepo;
        _sessionStore = sessionStore;
    }

    public async Task<RouletteDto> JoinRouletteSession(Guid userId)
    {
        var existing = _sessionStore.GetSessionForUser(userId);
        if (existing != null)
        {
            await ResolveRoundIfExpired(existing);
            return BuildDto(existing);
        }

        var session = _sessionStore.GetSessionWithCapacity(MaxUsersPerSession);
        if (session == null)
        {
            session = _sessionStore.CreateSession();
            lock (session.SyncRoot)
            {
                session.Status = RouletteSessionStatus.BettingOpen;
                session.RoundStartedUtc = DateTime.UtcNow;
            }

            var roundDto = new RoundDto
            {
                ID = Guid.NewGuid(),
                RoundPlayDate = DateTime.UtcNow,
                OutcomeId = null,
                OutcomeDescription = null,
                TotalAmount = null,
                Payout = null,
                Earnings = null,
                Bets = new List<Guid>()
            };

            await _roundRepo.PostAsync(roundDto);
            session.CurrentRoundId = roundDto.ID;
        }
        else
        {
            await ResolveRoundIfExpired(session);

            // Hvis round blev reset i Resolve og der af en eller anden grund ikke findes ID
            if (session.CurrentRoundId == null)
            {
                var roundDto = new RoundDto
                {
                    ID = Guid.NewGuid(),
                    RoundPlayDate = DateTime.UtcNow,
                    OutcomeId = null,
                    OutcomeDescription = null,
                    TotalAmount = null,
                    Payout = null,
                    Earnings = null,
                    Bets = new List<Guid>()
                };

                await _roundRepo.PostAsync(roundDto);
                session.CurrentRoundId = roundDto.ID;
            }
        }

        _sessionStore.AddUserToSession(session, userId);
        return BuildDto(session);
    }

    public Task<RouletteDto> LeaveRouletteSession(Guid userId)
    {
        var session = _sessionStore.GetSessionForUser(userId)
            ?? throw new System.Collections.Generic.KeyNotFoundException("User is not in any roulette session.");

        _sessionStore.RemoveUserFromSession(session, userId);

        return Task.FromResult(BuildDto(session));
    }

    public async Task<RouletteDto> PlaceRouletteBet(Guid userId, RouletteBetDto bet)
    {
        var session = _sessionStore.GetSessionForUser(userId)
            ?? throw new System.Collections.Generic.KeyNotFoundException("User is not in any roulette session.");

        await ResolveRoundIfExpired(session);

        if (bet == null)
            throw new ArgumentNullException(nameof(bet));

        if (bet.Amount <= 0)
            throw new ArgumentException("Bet amount must be greater than zero.");

        Guid currentRoundId;
        lock (session.SyncRoot)
        {
            if (session.Status != RouletteSessionStatus.BettingOpen)
                throw new InvalidOperationException("Betting is closed for this round.");

            if (session.CurrentRoundId == null)
                throw new InvalidOperationException("Round not initialized.");

            currentRoundId = session.CurrentRoundId.Value;
        }

        var user = await _userRepo.GetByIdAsync(userId);
        if (user.Saldo < bet.Amount)
            throw new InvalidOperationException("You cant afford this bet");

        // Træk saldo ved bet placement
        user.Saldo -= bet.Amount;
        await _userRepo.PutAsync(userId, user);

        lock (session.SyncRoot)
        {
            session.Bets.Add((userId, bet));
        }

        // Persist bet (antager OutcomeID matcher RouletteBetType enum-værdier)
        var betDto = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = userId,
            RoundId = currentRoundId,
            Amount = bet.Amount,
            OutcomeID = (int)bet.BetType,
            OutcomeDescription = bet.BetType.ToString()
        };

        await _betRepo.PostAsync(betDto);

        return BuildDto(session);
    }

    private async Task ResolveRoundIfExpired(RouletteSessionState session)
    {
        Guid currentRoundId;
        lock (session.SyncRoot)
        {
            var elapsed = (DateTime.UtcNow - session.RoundStartedUtc).TotalSeconds;
            if (elapsed <= session.RoundDurationSeconds)
                return;

            if (session.CurrentRoundId == null)
                throw new InvalidOperationException("Round not initialized.");

            session.Status = RouletteSessionStatus.Spinning;
            currentRoundId = session.CurrentRoundId.Value;
        }

        var winningNumber = Random.Shared.Next(0, 37);
        var winningColor = MapNumberToColor(winningNumber);

        lock (session.SyncRoot)
        {
            session.WinningNumber = winningNumber;
            session.WinningColor = winningColor;
        }

        // Hent persisted bets for denne round
        var roundBets = await _betRepo.GetAllBetsForRound(currentRoundId);

        var totalAmount = roundBets.Sum(b => b.Amount);
        var totalPayout = 0;

        foreach (var bet in roundBets)
        {
            // Antager OutcomeID matcher enum-værdierne:
            // Green=0, Red=1, Black=2
            var betType = (RouletteBetType)bet.OutcomeID;
            var isWin = betType == winningColor;

            if (!isWin)
                continue;

            var payout = betType == RouletteBetType.Green
                ? bet.Amount * 14
                : bet.Amount * 2;

            totalPayout += payout;

            var user = await _userRepo.GetByIdAsync(bet.UserId);
            user.Saldo += payout;
            await _userRepo.PutAsync(bet.UserId, user);
        }

        lock (session.SyncRoot)
        {
            session.Status = RouletteSessionStatus.RoundFinished;
        }

        // Opdater afsluttet round i DB
        var roundUpdate = new RoundDto
        {
            ID = currentRoundId,
            RoundPlayDate = DateTime.UtcNow,
            OutcomeId = (int)winningColor,
            OutcomeDescription = winningColor.ToString(),
            TotalAmount = totalAmount,
            Payout = totalPayout,
            Earnings = totalAmount - totalPayout,
            Bets = roundBets.Select(b => b.ID).ToList()
        };

        await _roundRepo.PutAsync(roundUpdate.ID, roundUpdate);

        // Start ny round
        lock (session.SyncRoot)
        {
            session.Bets.Clear();
            session.RoundStartedUtc = DateTime.UtcNow;
            session.Status = RouletteSessionStatus.BettingOpen;
            session.WinningNumber = null;
            session.WinningColor = null;
        }

        var nextRound = new RoundDto
        {
            ID = Guid.NewGuid(),
            RoundPlayDate = DateTime.UtcNow,
            OutcomeId = null,
            OutcomeDescription = null,
            TotalAmount = null,
            Payout = null,
            Earnings = null,
            Bets = new List<Guid>()
        };

        await _roundRepo.PostAsync(nextRound);
        lock (session.SyncRoot)
        {
            session.CurrentRoundId = nextRound.ID;
        }
    }

    private static RouletteBetType MapNumberToColor(int n)
    {
        if (n == 0) return RouletteBetType.Green;
        return RedNumbers.Contains(n) ? RouletteBetType.Red : RouletteBetType.Black;
    }

    private static RouletteDto BuildDto(RouletteSessionState s)
    {
        lock (s.SyncRoot)
        {
            return new RouletteDto
            {
                SessionId = s.SessionId,
                Status = s.Status,
                ActiveUsers = s.Participants.Count,
                MaxUsers = MaxUsersPerSession,
                SecondsLeft = Math.Max(
                    0,
                    s.RoundDurationSeconds - (int)(DateTime.UtcNow - s.RoundStartedUtc).TotalSeconds
                ),
                WinningNumber = s.WinningNumber,
                WinningColor = s.WinningColor,
                Participants = s.Participants.ToList()
            };
        }
    }
}