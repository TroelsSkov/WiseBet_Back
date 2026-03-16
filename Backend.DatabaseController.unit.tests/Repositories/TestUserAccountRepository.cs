using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using Microsoft.Data.Sqlite;
namespace Backend.DatabaseController.unit.tests.Repositories;

public class Tests
{
    private UserAccountRepository m_uut;
    private DatabaseContext m_context;
    private SqliteConnection m_connection;

    [SetUp]
    public void Setup()
    {
        m_connection = new SqliteConnection("DataSource=:memory:");
        m_connection.Open();

        var options = new DbContextOptionsBuilder<DatabaseContext>()
        .UseSqlite(m_connection)
        .Options;

        m_context = new DatabaseContext(options);

        m_context.Database.EnsureCreated();

        m_uut = new UserAccountRepository(m_context);

    }

    [Test]
    public async Task Get_EmptyListFromDb_ReturnsEmptyList()
    {
        var users = await m_uut.GetAllAsync();
        Assert.That(users.Count, Is.EqualTo(0));
    }

    [TearDown]
    public void TearDown()
    {
        m_context.Dispose();
        m_connection.Close();
    }

}
