using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reflection;
using WiseBet.backend.Data;
using WiseBet.backend.IRepository;
using WiseBet.backend.Models;
using WiseBet.backend.Services.DTOs;
using WiseBet.backend.Services.Roulette;

namespace Backend.DatabaseController.unit.tests.GameTests;

[TestFixture]
public class RouletteTest
{
    private RouletteService _uut;
    private UserAccountRepository _userRepo;
    private RoundRepository _roundRepo;
    private BetRepository _betRepo;
    private RouletteSessionStore _store;
    private DatabaseContext _context;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new DatabaseContext(options);
        _context.Outcomes.AddRange(
            new Outcome { OutcomeDescription = "Rød" },
            new Outcome { OutcomeDescription = "Sort" },
            new Outcome { OutcomeDescription = "Grøn" }
        );
        _context.SaveChanges();

        _userRepo = new UserAccountRepository(_context);
        _roundRepo = new RoundRepository(_context);
        _betRepo = new BetRepository(_context);
        _store = new RouletteSessionStore();

        _uut = new RouletteService(_userRepo, _roundRepo, _betRepo, _context, _store, NullLogger<RouletteService>.Instance);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task JoinRouletteSession_SameUserTwice_ReturnsSameSession()
    {
        var userId = Guid.NewGuid();

        var firstJoin = await _uut.JoinRouletteSession(userId);
        var secondJoin = await _uut.JoinRouletteSession(userId);

        Assert.That(secondJoin.Current.SessionId, Is.EqualTo(firstJoin.Current.SessionId));
        Assert.That(secondJoin.Current.ActiveUsers, Is.EqualTo(1));
    }

    [Test]
    public async Task JoinRouletteSession_SixthUser_GetsNewSession()
    {
        var users = Enumerable.Range(0, 6).Select(_ => Guid.NewGuid()).ToList();

        RouletteDto firstSession = null!;
        RouletteDto sixthSession = null!;

        for (var i = 0; i < users.Count; i++)
        {
            var update = await _uut.JoinRouletteSession(users[i]);
            if (i == 0) firstSession = update.Current;
            if (i == 5) sixthSession = update.Current;
        }

        var refreshedFirstSession = await _uut.JoinRouletteSession(users[0]);

        Assert.That(firstSession.SessionId, Is.Not.EqualTo(sixthSession.SessionId));
        Assert.That(refreshedFirstSession.Current.ActiveUsers, Is.EqualTo(5));
        Assert.That(sixthSession.ActiveUsers, Is.EqualTo(1));
    }

    [Test]
    public async Task LeaveRouletteSession_LastUserRemoved_CannotBetAfterwards()
    {
        var userId = Guid.NewGuid();
        await _uut.JoinRouletteSession(userId);
        await _uut.LeaveRouletteSession(userId);

        var bet = new RouletteBetDto { Amount = 10, BetType = RouletteBetType.Red };

        Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(
            () => _uut.PlaceRouletteBet(userId, bet));
    }

    [Test]
    public void PlaceRouletteBet_InvalidAmount_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var bet = new RouletteBetDto { Amount = 0, BetType = RouletteBetType.Black };

        Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await _uut.JoinRouletteSession(userId);
            await _uut.PlaceRouletteBet(userId, bet);
        });
    }

    [Test]
    public async Task PlaceRouletteBet_ValidBet_DecreasesSaldo_AndPersistsBet()
    {
        var userId = Guid.NewGuid();
        _context.UserAccounts.Add(new UserAccount { UserID = userId, Username = "Test", Saldo = 100 });
        await _context.SaveChangesAsync();

        await _uut.JoinRouletteSession(userId);
        var bet = new RouletteBetDto { Amount = 25, BetType = RouletteBetType.Green };

        await _uut.PlaceRouletteBet(userId, bet);

        var updatedUser = await _userRepo.GetByIdAsync(userId);
        Assert.That(updatedUser.Saldo, Is.EqualTo(75));

        Assert.That(_context.BetHistories.Count(), Is.EqualTo(1));
    }

    [Test]
    public async Task ProcessSessionTimers_ExpiredRound_IncludesRoundFinishedDtoWithWinningNumber()
    {
        var userId = Guid.NewGuid();
        _context.UserAccounts.Add(new UserAccount { UserID = userId, Username = "T", Saldo = 1000 });
        await _context.SaveChangesAsync();

        await _uut.JoinRouletteSession(userId);
        var session = _store.GetSessionForUser(userId)!;
        lock (session.SyncRoot)
        {
            session.RoundStartedUtc = DateTime.UtcNow.AddSeconds(-120);
        }

        var dtos = await _uut.ProcessSessionTimersAsync();

        var finished = dtos.FirstOrDefault(d =>
            d.Status == RouletteSessionStatus.RoundFinished && d.WinningNumber is >= 0 and <= 36);
        Assert.That(finished, Is.Not.Null, "Forventet RoundFinished med winningNumber før ny BettingOpen.");

        Assert.That(dtos.Any(d => d.Status == RouletteSessionStatus.BettingOpen && d.WinningNumber == null),
            Is.True, "Forventet ny runde uden vindertal efter resolve.");
    }

    [TestCase(0, RouletteBetType.Green)]
    [TestCase(1, RouletteBetType.Red)]
    [TestCase(2, RouletteBetType.Black)]
    [TestCase(3, RouletteBetType.Red)]
    [TestCase(4, RouletteBetType.Black)]
    [TestCase(12, RouletteBetType.Red)]
    [TestCase(11, RouletteBetType.Black)]
    [TestCase(36, RouletteBetType.Red)]
    public void MapNumberToColor_KnownNumbers_ReturnExpectedColor(int number, RouletteBetType expected)
    {
        var method = typeof(RouletteService).GetMethod("MapNumberToColor", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.That(method, Is.Not.Null);

        var actual = (RouletteBetType)method!.Invoke(null, new object[] { number })!;

        Assert.That(actual, Is.EqualTo(expected));
    }
}
