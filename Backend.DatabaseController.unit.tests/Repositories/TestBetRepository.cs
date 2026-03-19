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
            Password = "pass",
            Saldo = 100
        };

        u2 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "TestUser2",
            Password = "pass",
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
}
