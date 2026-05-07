using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WiseBet.backend.DTOs;
using WiseBet.backend.Services;
using WiseBet.backend.Services.DTOs;
using NUnit.Framework.Internal;
using WiseBet.backend.Hubs;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

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
    private Guid _userID;

    [SetUp]
    public void Setup()
    {
        _userID = Guid.NewGuid();

        _Icoinflip = Substitute.For<ICoinflipService>();
        _Iblackjack = Substitute.For<IBlackjackService>();
        _Ivalidation = Substitute.For<IGeneralValidation>();
        _Icaller = Substitute.For<IHubCallerClients>();
        _Iproxy = Substitute.For<ISingleClientProxy>();
        _Icaller.Caller.Returns(_Iproxy);

        var claims = new List<Claim> { new Claim("UserRepoConnect", _userID.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var mockContext = Substitute.For<HubCallerContext>();
        mockContext.User.Returns(claimsPrincipal);

        _hub = new GameHub(_Icoinflip, _Ivalidation, _Iblackjack)
        {
            Clients = _Icaller,
            Context = mockContext
        };
    }

    private void MockUserContext(Guid UserId)
    {
        var claims = new[] { new Claim("UserRepoConnect", UserId.ToString()) };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        var MockContext = Substitute.For<HubCallerContext>();
        MockContext.User.Returns(principal);
        _hub.Context = MockContext;
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
        MockUserContext(userID);
        var fail = new CoinFlipDTO { Fail = true, Message = "Mistake Found" };
        _Ivalidation.ValidateBet(userID, 100).Returns(fail);
        await _hub.PlayRound(100, CoinSide.Coin);

        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient",
        Arg.Is<object[]>(o => o[0].ToString() == "Mistake Found"), default);
        await _Icoinflip.DidNotReceive().PlayRound(userID, 100, CoinSide.Coin);
    }

    [Test]
    public async Task PlayRound_ValidationAccepts_BetIsAccepted()
    {
        Guid userID = Guid.NewGuid();
        MockUserContext(userID);
        _Ivalidation.ValidateBet(userID, 100).Returns(new CoinFlipDTO { Fail = false, Message = "BetAccepted" });

        await _hub.PlayRound(100, CoinSide.Coin);

        await _Icoinflip.Received().PlayRound(userID, 100, CoinSide.Coin);
    }

    // Blackjack tests
    [Test]
    public async Task StartRoundBlackjack_ValidationFails_ErrorSentToClient()
    {
        var fail = new CoinFlipDTO { Fail = true, Message = "Mistake Found" };
        _Ivalidation.ValidateBet(_userID, 100).Returns(fail);

        await _hub.StartRoundBlackjack(100);

        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient",
            Arg.Is<object[]>(o => o[0].ToString() == "Mistake Found"), default);
    }

    [Test]
    public async Task StartRoundBlackjack_ValidationFails_PlayBJRoundNotCalled()
    {
        var fail = new CoinFlipDTO { Fail = true, Message = "Mistake Found" };
        _Ivalidation.ValidateBet(_userID, 100).Returns(fail);

        await _hub.StartRoundBlackjack(100);

        await _Iblackjack.DidNotReceive().PlayBJRound(
            Arg.Any<ISingleClientProxy>(),
            Arg.Any<CancellationToken>(),
            _userID,
            100);
    }

    [Test]
    public async Task StartRoundBlackjack_ValidationAccepts_PlayBJRoundIsCalled()
    {
        _Ivalidation.ValidateBet(_userID, 100).Returns(new CoinFlipDTO { Fail = false, Message = "" });
        _Iblackjack.PlayBJRound(
            Arg.Any<ISingleClientProxy>(),
            Arg.Any<CancellationToken>(),
            _userID,
            100).Returns(Task.CompletedTask);

        await _hub.StartRoundBlackjack(100);

        await _Iblackjack.Received().PlayBJRound(
            Arg.Any<ISingleClientProxy>(),
            Arg.Any<CancellationToken>(),
            _userID,
            100);
    }

    [Test]
    public async Task StartRoundBlackjack_ServiceThrows_ErrorSentToClient()
    {
        _Ivalidation.ValidateBet(_userID, 100).Returns(new CoinFlipDTO { Fail = false, Message = "" });
        _Iblackjack.PlayBJRound(
            Arg.Any<ISingleClientProxy>(),
            Arg.Any<CancellationToken>(),
            _userID,
            100).Throws(new Exception("Service fejl"));

        await _hub.StartRoundBlackjack(100);

        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient",
            Arg.Is<object[]>(o => o[0].ToString() == "Service fejl"), default);
    }
}