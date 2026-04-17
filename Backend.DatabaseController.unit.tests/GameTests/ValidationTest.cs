using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using WiseBet.backend.Services;
using NSubstitute;
using WiseBet.backend.Controllers.DTOs;
using NUnit.Framework.Internal;
namespace Backend.DatabaseController.unit.tests.Hubs;
[TestFixture]
public class ValidationTest
{
    private GeneralValidation _uut;
    private UserAccountRepository repo;

    [SetUp]
    public void Setup()
    {
    repo = Substitute.For<UserAccountRepository>((DatabaseContext)null);
    _uut = new GeneralValidation(repo);
    }

    [Test]
    public async Task ValidateBet_TroelsSpiller_SendError()
    {
        Guid userId = Guid.NewGuid();
        var Troels = new UserAccountDto { ID = userId, Saldo = 1, };
        repo.GetByIdAsync(userId).Returns(Troels);

        var result = await _uut.ValidateBet(userId, 5);
        Assert.That(result.Fail, Is.True);
        Assert.That(result.Message, Is.EqualTo("You cant afford this bet"));
    }

    [Test]
    public async Task ValidateBet_BetIsLessOrEqualToZero_SendError()
    {
        Guid userId = Guid.NewGuid();
        var Troels = new UserAccountDto { ID = userId, Saldo = 100, };
        repo.GetByIdAsync(userId).Returns(Troels);

        var result = await _uut.ValidateBet(userId, -5);
        Assert.That(result.Fail, Is.True);
        Assert.That(result.Message, Is.EqualTo("Amount is less or equal to zero"));
    }

    [Test]
    public async Task ValidateBet_UserIsNull_SendError()
    {
        Guid userId = Guid.NewGuid();
        repo.GetByIdAsync(userId).Returns((UserAccountDto)null);
        var result = await _uut.ValidateBet(userId, 10);
        Assert.That(result.Fail, Is.True);
        Assert.That(result.Message, Is.EqualTo("User doesnt exist"));
    }

    [Test]
    public async Task ValidateBet_BetIsAccepted_AcceptBet()
    {
        Guid userId = Guid.NewGuid();
        var Troels = new UserAccountDto { ID = userId, Saldo = 100, };
        repo.GetByIdAsync(userId).Returns(Troels);

        var result = await _uut.ValidateBet(userId, 10);
        Assert.That(result.Fail, Is.False);
        Assert.That(result.Message, Is.EqualTo("Bet accepted"));
    }
}