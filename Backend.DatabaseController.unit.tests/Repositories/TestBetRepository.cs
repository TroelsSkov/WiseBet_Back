using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using WiseBet.backend.Models;
namespace Backend.DatabaseController.unit.tests.Repositories;

public class TestBetRepository
{
    BetRepository m_uut;
    private DatabaseContext m_context;
    private SqliteConnection m_connection;
    UserAccountRepository m_userrepo;
    UserAccountDto u1;
    UserAccountDto u2;
    Outcome o1;
    Outcome o2;
    Outcome o3;
    RoundRepository m_roundrepo;
    RoundDto r1;
    RoundDto r2;
    [SetUp]
    public async Task Setup()
    {
        m_connection = new SqliteConnection("DataSource=:memory:");
        m_connection.Open();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
        .UseSqlite(m_connection)
        .Options;

        m_context = new DatabaseContext(options);

        m_context.Database.EnsureCreated();

        m_uut = new BetRepository(m_context);

        m_userrepo = new UserAccountRepository(m_context);
        // Adding users to UserRepo
        u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "TestUser1",
            Saldo = 100
        };

        u2 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "TestUser2",
            Saldo = 200
        };

        await m_userrepo.PostAsync(u1);
        await m_userrepo.PostAsync(u2);

        // Seeding outcomes
        o1 = new Outcome
        {
            OutcomeDescription = "Rød"
        };

        o2 = new Outcome
        {
            OutcomeDescription = "Sort"
        };

        o3 = new Outcome
        {
            OutcomeDescription = "Grøn"
        };

        await m_context.AddRangeAsync(o1, o2, o3);
        await m_context.SaveChangesAsync();

        m_roundrepo = new RoundRepository(m_context);

        r1 = new RoundDto
        {
            ID = Guid.NewGuid(),
            OutcomeId = 1,
            OutcomeDescription = o2.OutcomeDescription
        };
        r2 = new RoundDto
        {
            ID = Guid.NewGuid(),
            OutcomeId = 2,
            OutcomeDescription = o3.OutcomeDescription
        };

        await m_roundrepo.PostAsync(r1);
        await m_roundrepo.PostAsync(r2);
    }

    [TearDown]
    public void TearDown()
    {
        m_context.Dispose();
        m_connection.Close();
    }

    [Test]
    public async Task Post_PostBetWithExisingForeignKeys_Succes()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };

        Assert.DoesNotThrowAsync(async () => await m_uut.PostAsync(b1));
    }
    [Test]
    public async Task Post_PostBetByIncorrectUserId_ThrowsInvalidOperation()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };

        Assert.ThrowsAsync<DbUpdateException>(async () => await m_uut.PostAsync(b1));
    }
    [Test]
    public async Task Post_PostBetIncorrectRoundID_ThrowsInvalidOperation()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = Guid.NewGuid(),
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };

        Assert.ThrowsAsync<DbUpdateException>(async () => await m_uut.PostAsync(b1));
    }
    [Test]
    public async Task Post_PostBetIncorrectOutcome_ThrowsInvalidOperation()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = -1,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };

        Assert.ThrowsAsync<DbUpdateException>(async () => await m_uut.PostAsync(b1));
    }

    [Test]
    public async Task Get_GetAllBets_ListCountIsTwo()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        BetDto b2 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            RoundId = r2.ID,
            OutcomeID = o3.OutcomeId,
            OutcomeDescription = o3.OutcomeDescription,
            Amount = 200
        };
        await m_uut.PostAsync(b1);
        await m_uut.PostAsync(b2);

        var bets = await m_uut.GetAllAsync();
        Assert.That(bets.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Get_GetAllBetsEmptyDb_ReturnsEmptyList()
    {
        var bets = await m_uut.GetAllAsync();
        Assert.That(bets, Is.Not.Null);
        Assert.That(bets.Count, Is.EqualTo(0));
    }
    [Test]
    public async Task Get_GetExsistingBetById_ReturnBetDto()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        BetDto b2 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            RoundId = r2.ID,
            OutcomeID = o3.OutcomeId,
            OutcomeDescription = o3.OutcomeDescription,
            Amount = 200
        };
        await m_uut.PostAsync(b1);
        await m_uut.PostAsync(b2);

        var bet = await m_uut.GetByIdAsync(b1.ID);
        Assert.That(bet, Is.Not.Null);
        Assert.That(bet.ID, Is.EqualTo(b1.ID));
    }

    [Test]
    public async Task Get_GetNonExsistingBet_ThrowsKeyNotFoundException()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        BetDto b2 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            RoundId = r2.ID,
            OutcomeID = o3.OutcomeId,
            OutcomeDescription = o3.OutcomeDescription,
            Amount = 200
        };
        await m_uut.PostAsync(b1);
        await m_uut.PostAsync(b2);

        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.GetByIdAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task Get_GetAllBetsFromExistingRound_ListCountIsTwo()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        BetDto b2 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            RoundId = r1.ID,
            OutcomeID = o3.OutcomeId,
            OutcomeDescription = o3.OutcomeDescription,
            Amount = 200
        };
        BetDto b3 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r2.ID,
            OutcomeID = o3.OutcomeId,
            OutcomeDescription = o3.OutcomeDescription,
            Amount = 200
        };

        await m_uut.PostAsync(b1);
        await m_uut.PostAsync(b2);
        await m_uut.PostAsync(b3);

        var betsR1 = await m_uut.GetAllBetsForRound(r1.ID);
        Assert.That(betsR1, Is.Not.Null);
        Assert.That(betsR1.Count, Is.EqualTo(2));
    }
    [Test]
    public async Task Get_GetAllBetsFromNonExsistingRound_ReturnsEmptyList()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        BetDto b2 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            RoundId = r2.ID,
            OutcomeID = o3.OutcomeId,
            OutcomeDescription = o3.OutcomeDescription,
            Amount = 200
        };

        await m_uut.PostAsync(b1);
        await m_uut.PostAsync(b2);

        var bets = await m_uut.GetAllBetsForRound(Guid.NewGuid());
        Assert.That(bets, Is.Not.Null);
        Assert.That(bets.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Put_ChangeAmountOfExsisingBet_ValuesUpdated()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        await m_uut.PostAsync(b1);

        b1.Amount = 1000;

        Assert.DoesNotThrowAsync(async () => await m_uut.PutAsync(b1.ID, b1));
        var bet = await m_uut.GetByIdAsync(b1.ID);

        Assert.That(bet.Amount, Is.EqualTo(1000));
    }
    [Test]
    public async Task Put_ChangeAmountOfNonExsisingBet_ThrowsKeyNotFoundException()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        await m_uut.PostAsync(b1);
        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.PutAsync(Guid.NewGuid(), b1));
    }
    [Test]
    public async Task Delete_DeleteExsistingBet_Sucess()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        await m_uut.PostAsync(b1);
        Assert.DoesNotThrowAsync(async () => await m_uut.DeleteAsync(b1));
    }
    [Test]
    public async Task Delete_DeleteExsistingBetTwice_ThrowsKeyNotFoundException()
    {
        BetDto b1 = new BetDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            RoundId = r1.ID,
            OutcomeID = o1.OutcomeId,
            OutcomeDescription = o1.OutcomeDescription,
            Amount = 200
        };
        await m_uut.PostAsync(b1);

        Assert.DoesNotThrowAsync(async () => await m_uut.DeleteAsync(b1));
        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.DeleteAsync(b1));
    }
}
