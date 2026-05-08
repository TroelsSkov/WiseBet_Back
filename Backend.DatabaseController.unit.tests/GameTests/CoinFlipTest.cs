using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using WiseBet.backend.Services;
using NSubstitute;
using WiseBet.backend.Services.DTOs;
using NUnit.Framework.Internal;
using System.Reflection;
using WiseBet.backend.Hubs;
namespace Backend.DatabaseController.unit.tests.Hubs;
[TestFixture]
public class CoinFlipTest
{
    private CoinFlipService _uut;
    private UserAccountRepository repo;
    private BetRepository betRepo;
    private RoundRepository roundRepo;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var fakeContext = new DatabaseContext(options);
        repo = Substitute.For<UserAccountRepository>(fakeContext);
        betRepo = Substitute.For<BetRepository>(fakeContext);
        roundRepo = Substitute.For<RoundRepository>(fakeContext);
        _uut = new CoinFlipService(repo, betRepo, roundRepo);
    }

    [Test]
    public async Task Playround_LegalAmount_CorrectLogicWin()
    {
        Guid userId = Guid.NewGuid();
        int InitialSaldo = 100;
        int betAmount = 50;
        var Troels = new UserAccountDto { ID = userId, Saldo = InitialSaldo, };
        repo.GetByIdAsync(userId).Returns(Troels);

        var result = await _uut.PlayRound(userId, betAmount, CoinSide.Wise);
        Assert.That(result.Fail, Is.False);
        Assert.That(result.Message, Is.AnyOf("You Won", "You almost won try again quickly"));
        Assert.That(result.Winnings, Is.AnyOf(100,0));
        if (result.Message == "You Won")
        {
            Assert.That(Troels.Saldo, Is.EqualTo(InitialSaldo+betAmount));
        }
        else
        {
            Assert.That(Troels.Saldo, Is.EqualTo(InitialSaldo-betAmount));
        }
    }
}  