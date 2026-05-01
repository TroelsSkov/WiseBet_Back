using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
using WiseBet.backend.IRepository;
using WiseBet.backend.Services.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace WiseBet.backend.Services.Roulette;

public class RouletteService : IRouletteService
{
    private readonly UserAccountRepository _userRepo;
    private readonly RoundRepository _roundRepo;
    private readonly BetRepository _betRepo;
    private readonly DatabaseContext _dbContext;
    private readonly RouletteSessionStore _sessionStore;
    private readonly ILogger<RouletteService> _logger;
    private const int MaxUsersPerSession = 5;
    private static readonly HashSet<int> RedNumbers = new()
    {
        1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
    };

    public RouletteService(
        UserAccountRepository userRepo,
        RoundRepository roundRepo,
        BetRepository betRepo,
        DatabaseContext dbContext,
        RouletteSessionStore sessionStore,
        ILogger<RouletteService> logger)
    {
        _userRepo = userRepo;
        _roundRepo = roundRepo;
        _betRepo = betRepo;
        _dbContext = dbContext;
        _sessionStore = sessionStore;
        _logger = logger;
    }

    public async Task<IReadOnlyList<RouletteDto>> ProcessSessionTimersAsync()
    {
        var snapshots = _sessionStore.SnapshotSessions();
        if (snapshots.Count == 0)
            return Array.Empty<RouletteDto>();

        var results = new List<RouletteDto>();
        foreach (var session in snapshots)
        {
            try
            {
                var fromResolve = await ResolveRoundIfExpired(session);
                results.AddRange(fromResolve);

                int participantCount;
                lock (session.SyncRoot)
                {
                    participantCount = session.Participants.Count;
                }

                if (participantCount == 0)
                    continue;

                results.Add(await BuildDtoAsync(session));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Roulette ProcessSessionTimers failed for session {SessionId}", session.SessionId);
            }
        }

        return results;
    }

    public async Task<RouletteSessionUpdate> JoinRouletteSession(Guid userId)
    {
        var existing = _sessionStore.GetSessionForUser(userId);
        if (existing != null)
        {
            await EnsureCurrentRoundAsync(existing);
            var broadcasts = await ResolveRoundIfExpired(existing);
            return new RouletteSessionUpdate(broadcasts, await BuildDtoAsync(existing));
        }

        var session = _sessionStore.TryJoinExistingSession(userId, MaxUsersPerSession);
        if (session != null)
        {
            await EnsureCurrentRoundAsync(session);
            var broadcasts = await ResolveRoundIfExpired(session);
            return new RouletteSessionUpdate(broadcasts, await BuildDtoAsync(session));
        }

        session = _sessionStore.CreateNewSessionWithUser(userId);
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
        lock (session.SyncRoot)
        {
            session.CurrentRoundId = roundDto.ID;
        }

        return new RouletteSessionUpdate(Array.Empty<RouletteDto>(), await BuildDtoAsync(session));
    }

    public async Task<RouletteSessionUpdate> LeaveRouletteSession(Guid userId)
    {
        var session = _sessionStore.GetSessionForUser(userId)
            ?? throw new System.Collections.Generic.KeyNotFoundException("User is not in any roulette session.");

        if (_sessionStore.IsLastParticipant(session, userId))
        {
            await ResolveRoundOnLastLeaveIfNeeded(session);
        }

        _sessionStore.RemoveUserFromSession(session, userId);

        return new RouletteSessionUpdate(Array.Empty<RouletteDto>(), await BuildDtoAsync(session));
    }

    public async Task<RouletteSessionUpdate> PlaceRouletteBet(Guid userId, RouletteBetDto bet)
    {
        var session = _sessionStore.GetSessionForUser(userId)
            ?? throw new System.Collections.Generic.KeyNotFoundException("User is not in any roulette session.");

        var broadcasts = await ResolveRoundIfExpired(session);

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

        var betDto = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = userId,
            RoundId = currentRoundId,
            Amount = bet.Amount,
            OutcomeID = await GetOutcomeIdAsync(bet.BetType),
            OutcomeDescription = bet.BetType.ToString()
        };

        if (string.Equals(_dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.InMemory", StringComparison.Ordinal))
        {
            user.Saldo -= bet.Amount;
            await _userRepo.PutAsync(userId, user);
            await _betRepo.PostAsync(betDto);
        }
        else
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                user.Saldo -= bet.Amount;
                await _userRepo.PutAsync(userId, user);
                await _betRepo.PostAsync(betDto);
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        lock (session.SyncRoot)
        {
            session.Bets.Add((userId, bet));
        }

        return new RouletteSessionUpdate(broadcasts, await BuildDtoAsync(session));
    }

    private async Task EnsureCurrentRoundAsync(RouletteSessionState session)
    {
        Guid? roundId;
        lock (session.SyncRoot)
        {
            roundId = session.CurrentRoundId;
        }

        if (roundId != null)
            return;

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
        lock (session.SyncRoot)
        {
            session.CurrentRoundId = roundDto.ID;
        }
    }

    // Returnerer 0 eller 1 DTO: færdigspillet runde med vindertal (før nulstilling til ny runde).

    private async Task<IReadOnlyList<RouletteDto>> ResolveRoundIfExpired(RouletteSessionState session)
    {
        Guid currentRoundId;
        lock (session.SyncRoot)
        {
            var elapsed = (DateTime.UtcNow - session.RoundStartedUtc).TotalSeconds;
            if (elapsed <= session.RoundDurationSeconds)
                return Array.Empty<RouletteDto>();

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

        var roundBets = await _betRepo.GetAllBetsForRound(currentRoundId);

        var totalAmount = roundBets.Sum(b => b.Amount);
        var totalPayout = 0;

        foreach (var bet in roundBets)
        {
            var betType = await MapOutcomeIdToBetTypeAsync(bet.OutcomeID);
            var isWin = betType == winningColor;

            if (!isWin)
                continue;

            var payout = betType == RouletteBetType.Green
                ? bet.Amount * 34
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

        var roundUpdate = new RoundDto
        {
            ID = currentRoundId,
            RoundPlayDate = DateTime.UtcNow,
            OutcomeId = await GetOutcomeIdAsync(winningColor),
            OutcomeDescription = winningColor.ToString(),
            TotalAmount = totalAmount,
            Payout = totalPayout,
            Earnings = totalAmount - totalPayout,
            Bets = roundBets.Select(b => b.ID).ToList()
        };

        await _roundRepo.PutAsync(roundUpdate.ID, roundUpdate);

        var finishedRoundDto = await BuildDtoAsync(session);

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

        return new[] { finishedRoundDto };
    }

    private static RouletteBetType MapNumberToColor(int n)
    {
        if (n == 0) return RouletteBetType.Green;
        return RedNumbers.Contains(n) ? RouletteBetType.Red : RouletteBetType.Black;
    }

    private async Task ResolveRoundOnLastLeaveIfNeeded(RouletteSessionState session)
    {
        Guid? roundId;
        lock (session.SyncRoot)
        {
            roundId = session.CurrentRoundId;
        }

        if (roundId == null)
            return;

        var round = await _roundRepo.GetByIdAsync(roundId.Value);
        if (round.OutcomeId != null)
            return;

        var hasPersistedBets = (await _betRepo.GetAllBetsForRound(roundId.Value)).Count > 0;
        if (!hasPersistedBets)
            return;

        lock (session.SyncRoot)
        {
            session.RoundStartedUtc = DateTime.UtcNow.AddSeconds(-session.RoundDurationSeconds - 1);
        }

        await ResolveRoundIfExpired(session);
    }

    private async Task<int> GetOutcomeIdAsync(RouletteBetType betType)
    {
        var descriptions = betType switch
        {
            RouletteBetType.Red => new[] { "Rød", "Rod", "Red" },
            RouletteBetType.Black => new[] { "Sort", "Black" },
            RouletteBetType.Green => new[] { "Grøn", "Gron", "Green" },
            _ => throw new ArgumentOutOfRangeException(nameof(betType), betType, "Unknown roulette bet type.")
        };

        var id = await _dbContext.Outcomes
            .Where(o => descriptions.Contains(o.OutcomeDescription))
            .Select(o => (int?)o.OutcomeId)
            .FirstOrDefaultAsync();

        if (id == null)
            throw new InvalidOperationException($"Outcome for {betType} not configured in database.");

        return id.Value;
    }

    private async Task<RouletteBetType> MapOutcomeIdToBetTypeAsync(int outcomeId)
    {
        var description = await _dbContext.Outcomes
            .Where(o => o.OutcomeId == outcomeId)
            .Select(o => o.OutcomeDescription)
            .FirstOrDefaultAsync();

        if (description == null)
            throw new ArgumentOutOfRangeException(nameof(outcomeId), outcomeId, "Unknown roulette outcome id.");

        return description switch
        {
            "Rød" or "Rod" or "Red" => RouletteBetType.Red,
            "Sort" or "Black" => RouletteBetType.Black,
            "Grøn" or "Gron" or "Green" => RouletteBetType.Green,
            _ => throw new ArgumentOutOfRangeException(nameof(outcomeId), outcomeId, "Unknown roulette outcome description.")
        };
    }

    private async Task<RouletteDto> BuildDtoAsync(RouletteSessionState s)
    {
        Guid sessionId;
        RouletteSessionStatus status;
        int activeUsers;
        int secondsLeft;
        int? winningNumber;
        RouletteBetType? winningColor;
        List<Guid> participants;
        List<(Guid UserId, RouletteBetDto Bet)> betsCopy;

        lock (s.SyncRoot)
        {
            sessionId = s.SessionId;
            status = s.Status;
            activeUsers = s.Participants.Count;
            secondsLeft = Math.Max(
                0,
                s.RoundDurationSeconds - (int)(DateTime.UtcNow - s.RoundStartedUtc).TotalSeconds
            );
            winningNumber = s.WinningNumber;
            winningColor = s.WinningColor;
            participants = s.Participants.ToList();
            betsCopy = s.Bets.ToList();
        }

        var entries = new List<RouletteBetEntryDto>(betsCopy.Count);
        foreach (var (uid, bet) in betsCopy)
        {
            var acc = await _userRepo.GetByIdAsync(uid);
            entries.Add(new RouletteBetEntryDto
            {
                UserId = uid,
                Username = acc?.Username ?? "?",
                Amount = bet.Amount,
                BetType = bet.BetType
            });
        }

        var totalRed = entries.Where(e => e.BetType == RouletteBetType.Red).Sum(e => e.Amount);
        var totalBlack = entries.Where(e => e.BetType == RouletteBetType.Black).Sum(e => e.Amount);
        var totalGreen = entries.Where(e => e.BetType == RouletteBetType.Green).Sum(e => e.Amount);

        return new RouletteDto
        {
            SessionId = sessionId,
            Status = status,
            ActiveUsers = activeUsers,
            MaxUsers = MaxUsersPerSession,
            SecondsLeft = secondsLeft,
            WinningNumber = winningNumber,
            WinningColor = winningColor,
            Participants = participants,
            CurrentRoundBets = entries,
            TotalOnRed = totalRed,
            TotalOnBlack = totalBlack,
            TotalOnGreen = totalGreen
        };
    }
}
