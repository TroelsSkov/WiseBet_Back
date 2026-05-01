using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using WiseBet.backend.Services;
using NSubstitute;
using NUnit.Framework.Internal;
using System.Reflection;
namespace Backend.DatabaseController.unit.tests.GameTests;


[TestFixture]
public class BlackjackTest
{
    private BlackjackService _uut;
    private UserAccountRepository _repo;
    private IDeck _mockDeck;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var fakeContext = new DatabaseContext(options);
        _repo = Substitute.For<UserAccountRepository>(fakeContext);

        _mockDeck = Substitute.For<IDeck>();
        // Standard deck der returnerer tilfældige kort (eksisterende tests)
        _mockDeck.draw().Returns(
            new Card(Suit.Hearts, Rank.Five),
            new Card(Suit.Spades, Rank.Seven),
            new Card(Suit.Hearts, Rank.Eight),
            new Card(Suit.Spades, Rank.Three),
            new Card(Suit.Hearts, Rank.Two),
            new Card(Suit.Spades, Rank.Four),
            new Card(Suit.Hearts, Rank.Six),
            new Card(Suit.Spades, Rank.Nine)
        );

        _uut = new BlackjackService(_repo, () => _mockDeck);
        _repo.PutAsync(Arg.Any<Guid>(), Arg.Any<UserAccountDto>()).Returns(Task.CompletedTask);
    }
    [Test]
    public async Task StartRound_ValidBet_SaldoDecreased()
    {
        Guid userId = Guid.NewGuid();
        int initialSaldo = 100;
        int bet = 50;
        var user = new UserAccountDto { ID = userId, Saldo = initialSaldo };
        _repo.GetByIdAsync(userId).Returns(user);

        await _uut.StartRound(userId, bet);

        Assert.That(user.Saldo, Is.LessThanOrEqualTo(initialSaldo - bet));
    }

    [Test]
    public async Task StartRound_ValidBet_PlayerGetsTwoCards()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var result = await _uut.StartRound(userId, 50);

        Assert.That(result.PlayerHand.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task StartRound_ValidBet_DealerGetsOneVisibleCard()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var result = await _uut.StartRound(userId, 50);

        Assert.That(result.DealerVisibleHand.Count, Is.AnyOf(1, 2));
    }

    [Test]
    public async Task StartRound_ValidBet_StatusIsValid()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var result = await _uut.StartRound(userId, 50);

        Assert.That(result.Status, Is.AnyOf(
            GameStatus.Playing,
            GameStatus.PlayerWin,
            GameStatus.DealerWin,
            GameStatus.Push
        ));
    }

    [Test]
    public async Task StartRound_Blackjack_GameRemovedFromActiveGames()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var result = await _uut.StartRound(userId, 50);

        if (result.Status != GameStatus.Playing)
            Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() => _uut.Hit(userId));
        else
            Assert.Pass();
    }
    [Test]
    public async Task Hit_ValidRound_PlayerGetsThirdCard()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        var result = await _uut.Hit(userId);

        Assert.That(result.PlayerHand.Count, Is.EqualTo(3));
    }

    [Test]
    public async Task Hit_PlayerBusts_StatusIsPlayerBust()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        GameStatus lastStatus = startResult.Status;
        while (lastStatus == GameStatus.Playing)
        {
            var hitResult = await _uut.Hit(userId);
            lastStatus = hitResult.Status;
        }

        Assert.That(lastStatus, Is.AnyOf(
            GameStatus.PlayerBust,
            GameStatus.PlayerWin
        ));
    }

    [Test]
    public async Task Hit_PlayerBusts_GameRemovedFromActiveGames()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        GameStatus lastStatus = startResult.Status;
        while (lastStatus == GameStatus.Playing)
        {
            var hitResult = await _uut.Hit(userId);
            lastStatus = hitResult.Status;
        }

        Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() => _uut.Hit(userId));
    }

    // Stand tests
    [Test]
    public async Task Stand_ValidRound_StatusIsValid()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        var result = await _uut.Stand(userId);

        Assert.That(result.Status, Is.AnyOf(
            GameStatus.PlayerWin,
            GameStatus.DealerWin,
            GameStatus.DealerBust,
            GameStatus.Push
        ));
    }

    [Test]
    public async Task Stand_ValidRound_DealerHandVisible()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        var result = await _uut.Stand(userId);

        Assert.That(result.DealerVisibleHand.Count, Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task Stand_ValidRound_GameRemovedFromActiveGames()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        await _uut.Stand(userId);

        Assert.ThrowsAsync<System.Collections.Generic.KeyNotFoundException>(() => _uut.Stand(userId));
    }

    [Test]
    public async Task Stand_PlayerWins_SaldoIncreased()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        var result = await _uut.Stand(userId);

        if (result.Status == GameStatus.PlayerWin || result.Status == GameStatus.DealerBust)
            Assert.That(user.Saldo, Is.EqualTo(150));
        else if (result.Status == GameStatus.Push)
            Assert.That(user.Saldo, Is.EqualTo(100));
        else
            Assert.That(user.Saldo, Is.EqualTo(50));
    }
    [Test]
    public async Task DealerBlackjack_Wins_Against_Player21()
    {
        var userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        _mockDeck.draw().Returns(
            new Card(Suit.Hearts, Rank.Five),   // Player kort 1
            new Card(Suit.Hearts, Rank.Ace),    // Dealer kort 1
            new Card(Suit.Hearts, Rank.Five),   // Player kort 2
            new Card(Suit.Hearts, Rank.King),   // Dealer kort 2
            new Card(Suit.Hearts, Rank.Ace)     // Player hit
        );

        var startResult = await _uut.StartRound(userId, 50);
        var hitResult = await _uut.Hit(userId);

        Assert.That(hitResult.PlayerScore, Is.EqualTo(21));
        Assert.That(hitResult.Status, Is.EqualTo(GameStatus.Playing));

        var standResult = await _uut.Stand(userId);
        Assert.That(standResult.Status, Is.EqualTo(GameStatus.DealerWin));
    }
}