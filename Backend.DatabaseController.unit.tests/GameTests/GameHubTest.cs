using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using WiseBet.backend.Services.Blackjack;
using WiseBet.backend.Services.Coinflip;
using WiseBet.backend.Services.Coinflip.Validation;
using WiseBet.backend.Services.DTOs;
using NUnit.Framework.Internal;
using WiseBet.backend.Hubs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Microsoft.Extensions.Configuration.UserSecrets;
namespace Backend.DatabaseController.unit.tests.Hubs;

[TestFixture]
public class GameHubTest
{
    private GameHub _hub;
    private ICoinflipService _Icoinflip;
    private IBlackjackService _Iblackjack;
    private IGeneralValidation _Ivalidation;
    private IHubCallerClients _Icaller;
    private ISingleClientProxy _Iproxy;

    [SetUp]
    public void Setup()
    {
        _Icoinflip = Substitute.For<ICoinflipService>();
        _Iblackjack = Substitute.For<IBlackjackService>();
        _Ivalidation = Substitute.For<IGeneralValidation>();
        _Icaller = Substitute.For<IHubCallerClients>();
        _Iproxy = Substitute.For<ISingleClientProxy>();
        _Icaller.Caller.Returns(_Iproxy);
        _hub = new GameHub(_Icoinflip, _Ivalidation, _Iblackjack)
        {
            Clients = _Icaller
        };
    }

    [TearDown]
    public void TearDown()
    {
        if (_hub is IDisposable disposableHub)
            disposableHub.Dispose();
    }

    // Coinflip tests
    [Test]
    public async Task PlayRound_ValidationFails_BetNotAccepted()
    {
        Guid userID = Guid.NewGuid();
        var fail = new CoinFlipDTO { Fail = true, Message = "Mistake Found" };
        _Ivalidation.ValidateBet(userID, 100).Returns(fail);

        await _hub.PlayRound(userID, 100, CoinSide.Coin);

        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient",
        Arg.Is<object[]>(o => o[0].ToString() == "Mistake Found"), default);
        await _Icoinflip.DidNotReceive().PlayRound(userID, 100, CoinSide.Coin);
    }

    [Test]
    public async Task PlayRound_ValidationAccepts_BetIsAccepted()
    {
        Guid userID = Guid.NewGuid();
        _Ivalidation.ValidateBet(userID, 100).Returns(new CoinFlipDTO { Fail = false, Message = "BetAccepted" });

        await _hub.PlayRound(userID, 100, CoinSide.Coin);

        await _Icoinflip.Received().PlayRound(userID, 100, CoinSide.Coin);
    }

    // Blackjack tests
    [Test]
    public async Task StartRoundBlackjack_ValidationFails_BetNotAccepted()
    {
        Guid userID = Guid.NewGuid();
        var fail = new CoinFlipDTO { Fail = true, Message = "Mistake Found" };
        _Ivalidation.ValidateBet(userID, 100).Returns(fail);

        await _hub.StartRoundBlackjack(userID, 100);

        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient",
        Arg.Is<object[]>(o => o[0].ToString() == "Mistake Found"), default);
        await _Iblackjack.DidNotReceive().StartRound(userID, 100);
    }

    [Test]
    public async Task StartRoundBlackjack_ValidationAccepts_ServiceIsCalled()
    {
        Guid userID = Guid.NewGuid();
        _Ivalidation.ValidateBet(userID, 100).Returns(new CoinFlipDTO { Fail = false, Message = ""  });
        _Iblackjack.StartRound(userID, 100).Returns(new BlackjackDto());

        await _hub.StartRoundBlackjack(userID, 100);

        await _Iblackjack.Received().StartRound(userID, 100);
    }

    [Test]
    public async Task HitBlackjack_ServiceIsCalled()
    {
        Guid userID = Guid.NewGuid();
        _Iblackjack.Hit(userID).Returns(new BlackjackDto());

        await _hub.HitBlackjack(userID);

        await _Iblackjack.Received().Hit(userID);
    }

    [Test]
    public async Task HitBlackjack_ServiceThrows_ErrorSentToClient()
    {
        Guid userID = Guid.NewGuid();
        _Iblackjack.Hit(userID).Throws(new Exception("Fejl"));

        await _hub.HitBlackjack(userID);

        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient",
        Arg.Is<object[]>(o => o[0].ToString() == "Fejl"), default);
    }

    [Test]
    public async Task StandBlackjack_ServiceIsCalled()
    {
        Guid userID = Guid.NewGuid();
        _Iblackjack.Stand(userID).Returns(new BlackjackDto());

        await _hub.StandBlackjack(userID);

        await _Iblackjack.Received().Stand(userID);
    }

    [Test]
    public async Task StandBlackjack_ServiceThrows_ErrorSentToClient()
    {
        Guid userID = Guid.NewGuid();
        _Iblackjack.Stand(userID).Throws(new Exception("Fejl"));

        await _hub.StandBlackjack(userID);

        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient",
        Arg.Is<object[]>(o => o[0].ToString() == "Fejl"), default);
    }
}