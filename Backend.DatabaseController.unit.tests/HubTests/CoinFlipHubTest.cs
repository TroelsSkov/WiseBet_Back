using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using WiseBet.backend.Models;
using WiseBet.backend.Hubs;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using NUnit.Framework;
using NSubstitute.Core.Arguments;
using WiseBet.backend.Controllers.DTOs;
namespace Backend.DatabaseController.unit.tests.Hubs;
[TestFixture]
public class CoinFlipHubTest
{
    private CoinFlipHub _uut;
    private IHubCallerClients _mockClients;
    private ISingleClientProxy _mockCallerProxy;

    [SetUp]
    public void Setup()
    {
        _mockClients = Substitute.For<IHubCallerClients>();
        _mockCallerProxy = Substitute.For<ISingleClientProxy>(); 
        _mockClients.Caller.Returns(_mockCallerProxy);
        _uut = new CoinFlipHub
        {
            Clients = _mockClients
        };
    }
//this is always used with hubs to free mo
    [TearDown]
    public void TearDown()
    {
        _uut?.Dispose();
    }

    [Test]
    public async Task PlayRound_IllegalAmount_SendError()
    {
        await _uut.PlayRound(-5, CoinSide.plat);
        await _mockCallerProxy.Received(1).SendCoreAsync("ErrorMessageToClient", Arg.Is<object[]>(args => args[0].ToString() == "Amount is less or equal to zero"));
    }

    [Test]
    public async Task Playround_LegalAmount_CorrectLogicWin()
    {
        int BetAmount = 100;
        CoinSide Chosen = CoinSide.krone;
        object[] CapturedArgs = null;
        
        await _mockCallerProxy.SendCoreAsync("UpdateClient", 
        Arg.Do<object[]>(args => CapturedArgs = args),
        Arg.Any<CancellationToken>());
        
        await _uut.PlayRound(BetAmount, Chosen);

        await _mockCallerProxy.Received(1).SendCoreAsync("UpdateClient", Arg.Any<object[]>(), Arg.Any<CancellationToken>());

        var result = CapturedArgs[0] as CoinFlipDTO;
        int LandingSide = result.LandingSide;
        int winnings = result.Winnings;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.LandingSide, Is.AnyOf(0,1));

        if (LandingSide == (int)Chosen)
        {
            Assert.That(winnings, Is.EqualTo(BetAmount*2));
        }
        else
        {
            Assert.That(winnings, Is.EqualTo(0));
        }
    }
    
    

}