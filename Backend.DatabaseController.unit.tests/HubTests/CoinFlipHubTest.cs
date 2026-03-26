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
//this is always used with hubs to free memory.
    [TearDown]
    public void TearDown()
    {
        _uut?.Dispose();
    }

    [Test]
    public async Task PlayRound_IllegalAmount_SendError()
    {
        await _uut.PlayRound(-5, CoinSide.plat);
        await _mockCallerProxy.Received(1).SendCoreAsync("RecieveError", Arg.Is<object[]>(args => args[0].ToString() == "Amount is less or equal to zero"));
    }
//todo test PlayRound_LegalAmount_CorrectRoundLogic
}