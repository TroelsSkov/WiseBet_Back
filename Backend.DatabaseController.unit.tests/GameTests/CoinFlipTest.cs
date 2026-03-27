using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using WiseBet.backend.Controllers.DTOs;
using WiseBet.backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using WiseBet.backend.Models;
using WiseBet.backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NUnit.Framework;
using NSubstitute.Core.Arguments;
using WiseBet.backend.Controllers.DTOs;
using NUnit.Framework.Internal;
namespace Backend.DatabaseController.unit.tests.Hubs;
[TestFixture]
public class CoinFlipTest
{
    private CoinFlipService _uut;
    private UserAccountRepository _mockRepo;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<DatabaseContext>()
            .UseInMemoryDatabase(databaseName: "TestDb")
            .Options;
        var fakeContext = new DatabaseContext(options);
        _mockRepo = Substitute.For<UserAccountRepository>(fakeContext);
        _uut = new CoinFlipService(_mockRepo);
    }

    [Test]
    public async Task PlayRound_TroelsSpiller_SendError()
    {
        Guid userId = Guid.NewGuid();
        var Troels = new UserAccountDto { ID = userId, Saldo = 1, };
        _mockRepo.GetByIdAsync(userId).Returns(Troels);

        var result = await _uut.PlayRound(userId, 5, CoinSide.plat);
        Assert.That(result.fail, Is.True);
        Assert.That(result.message, Is.EqualTo("You cant afford this bet"));
    }

    [Test]
    public async Task PlayRound_BetIsLessOrEqualToZero_SendError()
    {
        Guid userId = Guid.NewGuid();
        var Troels = new UserAccountDto { ID = userId, Saldo = 100, };
        _mockRepo.GetByIdAsync(userId).Returns(Troels);

        var result = await _uut.PlayRound(userId, -5, CoinSide.plat);
        Assert.That(result.fail, Is.True);
        Assert.That(result.message, Is.EqualTo("Amount is less or equal to zero"));
    }

    [Test]
    public async Task PlayRound_UserIsNull_SendError()
    {
        Guid userId = Guid.NewGuid();
        _mockRepo.GetByIdAsync(userId).Returns((UserAccountDto)null);
        var result = await _uut.PlayRound(userId, 10, CoinSide.plat);
        Assert.That(result.fail, Is.True);
        Assert.That(result.message, Is.EqualTo("User doesnt exist"));
    }



    [Test]
    public async Task Playround_LegalAmount_CorrectLogicWin()
    {
        Guid userId = Guid.NewGuid();
        var Troels = new UserAccountDto { ID = userId, Saldo = 100, };
        _mockRepo.GetByIdAsync(userId).Returns(Troels);

        var result = await _uut.PlayRound(userId, 50, CoinSide.plat);
        Assert.That(result.fail, Is.False);
        Assert.That(result.message, Is.AnyOf("You Won", "You almost won try again quickly"));
        Assert.That(result.Winnings, Is.AnyOf(100,0));
    }
    
    

}