using Moq;
using NUnit.Framework;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using WiseBet.backend.Services;
using WiseBet.backend.Controllers.DTOs;
using NUnit.Framework.Internal;
using WiseBet.backend.Hubs;
using NSubstitute;
using Microsoft.Extensions.Configuration.UserSecrets;
namespace Backend.DatabaseController.unit.tests.Hubs;

[TestFixture]
public class GameHubTest
{
    private GameHub _hub;
    private ICoinflipService _Icoinflip;
    private IGeneralValidation _Ivalidation;
    private IHubCallerClients _Icaller;
    private ISingleClientProxy _Iproxy;

    [SetUp]
    public void Setup()
    {
        _Icoinflip = Substitute.For<ICoinflipService>();
        _Ivalidation = Substitute.For<IGeneralValidation>();
        _Icaller = Substitute.For<IHubCallerClients>();
        _Iproxy = Substitute.For<ISingleClientProxy>();
        _Icaller.Caller.Returns(_Iproxy);
        _hub = new GameHub(_Icoinflip, _Ivalidation)
        {
            Clients = _Icaller
        };

    }

    [TearDown]
    public void TearDown()
    {
        if (_hub is IDisposable disposableHub)
        {
            disposableHub.Dispose();
        }
    }

    [Test]
    public async Task PlayRound_ValidationFails_BetNotAccepted()
    {
        Guid userID = Guid.NewGuid();
        var fail = new CoinFlipDTO {Fail = true, Message = "Mistake Found"};
        _Ivalidation.ValidateBet(userID, 100).Returns(fail);
        await _hub.PlayRound(userID, 100, CoinSide.Coin);
        
        await _Iproxy.Received().SendCoreAsync("ErrorMessageToClient", 
        Arg.Is<object[]>(o => o[0].ToString() == "Mistake Found"),default);

        await _Icoinflip.DidNotReceive().PlayRound(userID, 100, CoinSide.Coin);
    }

        [Test]
    public async Task PlayRound_ValidationAccepts_BetIsAccepted()
    {
        Guid userID = Guid.NewGuid();
        _Ivalidation.ValidateBet(userID, 100).Returns(new CoinFlipDTO{Fail = false, Message = "BetAccepted"});
        
        await _hub.PlayRound(userID, 100, CoinSide.Coin);
        

        await _Icoinflip.Received().PlayRound(userID, 100, CoinSide.Coin);
    }

}