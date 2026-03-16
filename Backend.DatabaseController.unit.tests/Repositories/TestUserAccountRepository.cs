using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
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

    [TearDown]
    public void TearDown()
    {
        m_context.Dispose();
        m_connection.Close();
    }


    [Test]
    public async Task Post_UpdateDatbaseWithUserDto_success()
    {
        UserAccountDto u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test1",
            Password = "Test1_Password",
            Saldo = 0
        };

        await m_uut.PostAsync(u1);
        var users = await m_uut.GetAllAsync();
        Assert.That(users.Count, Is.EqualTo(1));
    }


    [Test]
    public async Task Get_EmptyListFromDb_ReturnsEmptyList()
    {
        var users = await m_uut.GetAllAsync();
        Assert.That(users.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Get_ListWithUsers_ListCountOfTwo()
    {
        UserAccountDto u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test1",
            Password = "Test1_Password",
            Saldo = 0
        };

        UserAccountDto u2 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test2",
            Password = "Test2_Passwrod",
            Saldo = 100
        };

        await m_uut.PostAsync(u1);
        await m_uut.PostAsync(u2);

        var users = await m_uut.GetAllAsync();
        Assert.That(users.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Get_UserWithId_ReturnsUserWithGuid()
    {
        UserAccountDto u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test1",
            Password = "Test1_Password",
            Saldo = 0
        };

        await m_uut.PostAsync(u1);

        var user = await m_uut.GetByIdAsync(u1.ID);
        Assert.That(user, Is.Not.Null);
        Assert.That(user.ID, Is.EqualTo(u1.ID));
    }

    [Test]
    public async Task Get_UserWithId_ThrowsKeyNotFoundException()
    {
        UserAccountDto u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test1",
            Password = "Test1_Password",
            Saldo = 0
        };

        await m_uut.PostAsync(u1);

        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.GetByIdAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task Put_ChangeExistingUser_UserChanged()
    {
        UserAccountDto u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test1",
            Password = "Test1_Password",
            Saldo = 0
        };

        await m_uut.PostAsync(u1);
        var user = await m_uut.GetByIdAsync(u1.ID);

        Assert.That(user, Is.Not.Null);
        Assert.That(user.ID, Is.EqualTo(u1.ID));

        u1.Username = "Test1_changed";
        u1.Password = "Test1_Password_changed";
        u1.Saldo = 123;

        await m_uut.PutAsync(u1.ID, u1);

        var updatedUser = await m_uut.GetByIdAsync(u1.ID);

        Assert.That(updatedUser, Is.Not.Null);
        Assert.That(updatedUser.ID, Is.EqualTo(u1.ID));
        Assert.That(updatedUser.Username, Is.EqualTo(u1.Username));
        Assert.That(updatedUser.Password, Is.EqualTo(u1.Password));
        Assert.That(updatedUser.Saldo, Is.EqualTo(u1.Saldo));
    }

    [Test]
    public async Task Put_ChangeExistingUser_ThrowsKeyNotFoundException()
    {
        UserAccountDto u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test1",
            Password = "Test1_Password",
            Saldo = 0
        };

        await m_uut.PostAsync(u1);
        var user = await m_uut.GetByIdAsync(u1.ID);

        Assert.That(user, Is.Not.Null);
        Assert.That(user.ID, Is.EqualTo(u1.ID));

        u1.Username = "Test1_changed";
        u1.Password = "Test1_Password_changed";
        u1.Saldo = 123;

        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.PutAsync(Guid.NewGuid(), u1));
    }

    [Test]
    public async Task Delete_ExistingUserDeleted_UserDeleted()
    {
        UserAccountDto u1 = new UserAccountDto
        {
            ID = Guid.NewGuid(),
            Username = "Test1",
            Password = "Test1_Password",
            Saldo = 0
        };

        await m_uut.PostAsync(u1);

        var user = await m_uut.GetByIdAsync(u1.ID);
        Assert.That(user, Is.Not.Null);
        Assert.That(user.ID, Is.EqualTo(u1.ID));

        await m_uut.DeleteAsync(u1);

        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.DeleteAsync(u1));
    }
}
