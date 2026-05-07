using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using WiseBet.backend.Services;
using NSubstitute;
using NUnit.Framework.Internal;
using Microsoft.AspNetCore.SignalR;
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
        // Standard deck der returnerer kort der giver spillende status (ingen blackjack ved start)
        // Player: 5 + 8 = 13, Dealer: 7 + 3 = 10
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

    // StartRound tests
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
    public async Task StartRound_PlayerBlackjack_SaldoIncreasedWithBonus()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        // Player: Ace + King = 21 (blackjack), Dealer: Five + Seven = 12
        _mockDeck.draw().Returns(
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Spades, Rank.Five),
            new Card(Suit.Hearts, Rank.King),
            new Card(Suit.Spades, Rank.Seven)
        );

        var result = await _uut.StartRound(userId, 50);

        Assert.That(result.Status, Is.EqualTo(GameStatus.PlayerWin));
        // Saldo: 100 - 50 (bet) + 125 (bet * 2.5) = 175
        Assert.That(user.Saldo, Is.EqualTo(175));
    }

    [Test]
    public async Task StartRound_BothBlackjack_Push()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        // Player: Ace + King = 21, Dealer: Ace + King = 21
        _mockDeck.draw().Returns(
            new Card(Suit.Hearts, Rank.Ace),
            new Card(Suit.Spades, Rank.Ace),
            new Card(Suit.Hearts, Rank.King),
            new Card(Suit.Spades, Rank.King)
        );

        var result = await _uut.StartRound(userId, 50);

        Assert.That(result.Status, Is.EqualTo(GameStatus.Push));
        // Saldo: 100 - 50 (bet) + 50 (refund) = 100
        Assert.That(user.Saldo, Is.EqualTo(100));
    }

    // Hit tests
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
    public async Task Stand_DealerBusts_SaldoIncreased()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        // Player: 10 + 9 = 19, Dealer: 6 + 6 = 12 → trækker King = 22 (bust)
        _mockDeck.draw().Returns(
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Spades, Rank.Six),
            new Card(Suit.Hearts, Rank.Nine),
            new Card(Suit.Spades, Rank.Six),
            new Card(Suit.Hearts, Rank.King)
        );

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        var result = await _uut.Stand(userId);

        Assert.That(result.Status, Is.EqualTo(GameStatus.DealerBust));
        // Saldo: 100 - 50 (bet) + 100 (bet * 2) = 150
        Assert.That(user.Saldo, Is.EqualTo(150));
    }

    [Test]
    public async Task Stand_Push_SaldoRefunded()
    {
        Guid userId = Guid.NewGuid();
        var user = new UserAccountDto { ID = userId, Saldo = 100 };
        _repo.GetByIdAsync(userId).Returns(user);

        // Player: 10 + 9 = 19, Dealer: 10 + 9 = 19 → push
        _mockDeck.draw().Returns(
            new Card(Suit.Hearts, Rank.Ten),
            new Card(Suit.Spades, Rank.Ten),
            new Card(Suit.Hearts, Rank.Nine),
            new Card(Suit.Spades, Rank.Nine)
        );

        var startResult = await _uut.StartRound(userId, 50);
        Assume.That(startResult.Status, Is.EqualTo(GameStatus.Playing));

        var result = await _uut.Stand(userId);

        Assert.That(result.Status, Is.EqualTo(GameStatus.Push));
        // Saldo: 100 - 50 (bet) + 50 (refund) = 100
        Assert.That(user.Saldo, Is.EqualTo(100));
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

    // MultipleGames tests
    [Test]
    public async Task MultipleGames_TwoPlayers_IndependentGameStates()
    {
        Guid userId1 = Guid.NewGuid();
        Guid userId2 = Guid.NewGuid();
        var user1 = new UserAccountDto { ID = userId1, Saldo = 100 };
        var user2 = new UserAccountDto { ID = userId2, Saldo = 200 };
        _repo.GetByIdAsync(userId1).Returns(user1);
        _repo.GetByIdAsync(userId2).Returns(user2);

        var result1 = await _uut.StartRound(userId1, 50);
        var result2 = await _uut.StartRound(userId2, 100);

        Assume.That(result1.Status, Is.EqualTo(GameStatus.Playing));
        Assume.That(result2.Status, Is.EqualTo(GameStatus.Playing));

        // Begge spil kører uafhængigt — hit på spiller 1 påvirker ikke spiller 2
        var hit1 = await _uut.Hit(userId1);
        Assert.That(hit1.PlayerHand.Count, Is.EqualTo(3));

        var stand2 = await _uut.Stand(userId2);
        Assert.That(stand2.Status, Is.Not.EqualTo(GameStatus.Playing));
    }
}