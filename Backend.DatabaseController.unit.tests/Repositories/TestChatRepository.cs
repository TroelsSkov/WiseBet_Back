using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
namespace Backend.DatabaseController.unit.tests.Repositories;

public class TestChatsRepository
{
    private UserAccountDto u1;
    private UserAccountDto u2;
    private ChatsRepository m_uut;
    private UserAccountRepository m_userRepo;
    private DatabaseContext m_context;
    private SqliteConnection m_connection;

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

        m_uut = new ChatsRepository(m_context);

        m_userRepo = new UserAccountRepository(m_context);

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

        await m_userRepo.PostAsync(u1);
        await m_userRepo.PostAsync(u2);
    }

    [TearDown]
    public void TearDown()
    {
        m_context.Dispose();
        m_connection.Close();
    }

    [Test]
    public async Task Post_UpdateDatbaseWithChatDto_success()
    {
        ChatDto newChat = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        Assert.DoesNotThrowAsync(async () => await m_uut.PostAsync(newChat));
    }

    [Test]
    public async Task Post_SameChaIDPostedTwice_ThrowsException()
    {
        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        await m_uut.PostAsync(c1);

        Assert.ThrowsAsync<InvalidOperationException>(async () => await m_uut.PostAsync(c1));
    }

    [TestCase("Hello World!")]
    [TestCase("Hejsa alle sammen")]
    [TestCase("12345678910")]
    [TestCase("åøæ")]
    [TestCase(@"/*--*\")]
    public async Task Get_ChatsPostetMessageMatch_ChatStringMatches(string msg)
    {
        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        Assert.DoesNotThrowAsync(async () => await m_uut.PostAsync(c1));

        var chat = await m_uut.GetByIdAsync(c1.ID);
        Assert.That(chat.Message, Is.EqualTo(c1.Message));
    }

    [Test]
    public async Task GetAll_CheckingSizeOfListedChats_ListGrowsPerPost()
    {
        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        await m_uut.PostAsync(c1);

        var chats = await m_uut.GetAllAsync();
        Assert.That(chats.Count, Is.EqualTo(1));

        ChatDto c2 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        await m_uut.PostAsync(c2);

        chats = await m_uut.GetAllAsync();
        Assert.That(chats.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Get_GetByIdOnEmptyDb_ThrowsException()
    {
        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.GetByIdAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task Put_ChangeMessageOfExistingChat_Success()
    {
        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        await m_uut.PostAsync(c1);

        c1.Message = "I changed the message";

        Assert.DoesNotThrowAsync(async () => await m_uut.PutAsync(c1.ID, c1));

        var chat = await m_uut.GetByIdAsync(c1.ID);
        Assert.That(chat.Message, Is.EqualTo("I changed the message"));
    }

    [Test]
    public async Task Put_ChangeMessageOfNonExistingChat_ThrowsException()
    {
        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.PutAsync(c1.ID, c1));
    }

    [Test]
    public async Task Delete_DeleteExsistingChat_success()
    {
        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        await m_uut.PostAsync(c1);

        var chats = await m_uut.GetAllAsync();
        Assert.That(chats.Count, Is.EqualTo(1));

        Assert.DoesNotThrowAsync(async () => await m_uut.DeleteAsync(c1));
    }

    [Test]
    public async Task Delete_DeleteNonExsistingChat_ThrowsException()
    {

        var chats = await m_uut.GetAllAsync();
        Assert.That(chats.Count, Is.EqualTo(0));

        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Hello World",
            TimeOfChat = DateTime.Now
        };

        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.DeleteAsync(c1));
    }

    [Test]
    public async Task Get_GetUserChatsByUserId_ReturnsUserChats()
    {
        ChatDto c1 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Hey Aske",
            TimeOfChat = DateTime.Now
        };
        ChatDto c2 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u2.ID,
            Message = "Hey Katrine",
            TimeOfChat = DateTime.Now
        };
        ChatDto c3 = new ChatDto
        {
            ID = Guid.NewGuid(),
            UserId = u1.ID,
            Message = "Du lækker <3",
            TimeOfChat = DateTime.Now
        };

        await m_uut.PostAsync(c1);
        await m_uut.PostAsync(c2);
        await m_uut.PostAsync(c3);

        var u1Chats = await m_uut.GetAllUserChatsAsync(u1.ID);
        Assert.That(u1Chats.Count, Is.EqualTo(2));
        var u2Chats = await m_uut.GetAllUserChatsAsync(u2.ID);
        Assert.That(u2Chats.Count, Is.EqualTo(1));

        Assert.That(u1Chats[0].Message, Is.EqualTo(c1.Message));
        Assert.That(u1Chats[1].Message, Is.EqualTo(c3.Message));
        Assert.That(u2Chats[0].Message, Is.EqualTo(c2.Message));
    }

        [Test]
    public async Task Get_GetUserChatsByWrongUserId_ReturnsEmptyList()
    {
        var userChats = await m_uut.GetAllUserChatsAsync(Guid.NewGuid());
        Assert.That(userChats, Is.Not.Null);
        Assert.That(userChats.Count, Is.EqualTo(0));
    }
}
