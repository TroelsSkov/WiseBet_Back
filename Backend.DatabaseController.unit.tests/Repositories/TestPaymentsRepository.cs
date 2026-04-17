using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using Microsoft.EntityFrameworkCore;
using WiseBet.backend.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using WiseBet.backend.Models;
namespace Backend.DatabaseController.unit.tests.Repositories;

public class TestPaymentsRepository
{
    private PaymentsRepository m_uut;
    private UserAccountRepository m_userrepo;
    private UserAccountDto u1;
    private UserAccountDto u2;
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

        m_uut = new PaymentsRepository(m_context);
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
    }

    [TearDown]
    public void TearDown()
    {
        m_context.Dispose();
        m_connection.Close();
    }

    // [Test]
    // public async Task func()
    // {

    // }

    [Test]
    public async Task Post_UpdateDbWithNewPayment_Sucess()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 200
        };

        Assert.DoesNotThrowAsync(async () => await m_uut.PostAsync(p1));
    }

    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(200)]
    [TestCase(9999999)]
    public async Task Post_UpdateDbWithNewPayment_SaldoUpdatesForUser(int saldo)
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = saldo
        };

        Assert.DoesNotThrowAsync(async () => await m_uut.PostAsync(p1));
        var user = await m_userrepo.GetByIdAsync(p1.UserID);
        Assert.That(user.Saldo, Is.EqualTo(u1.Saldo + p1.PaymentAmount));
    }
    [Test]
    public async Task Post_UpdateDatabaseWithNegativePaymentAmount_ThrowsException()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = -1
        };

        Assert.ThrowsAsync<WiseBet.backend.IRepository.InvalidParameterException>(async () => await m_uut.PostAsync(p1));
    }

    [Test]
    public async Task Get_GetAllPaymentsFromNonEmptyDb_ReturnsNonEmptyList()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 100
        };
        PaymentDto p2 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u2.ID,
            PaymentAmount = 200
        };
        await m_uut.PostAsync(p1);
        await m_uut.PostAsync(p2);

        var payments = await m_uut.GetAllAsync();
        Assert.That(payments.Count, Is.EqualTo(2));
    }
    [Test]
    public async Task Get_GetAllPaymentsFromEmptyDb_ReturnsEmptyList()
    {

        var payments = await m_uut.GetAllAsync();
        Assert.That(payments.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task Get_GetExsistingPaymentById_ReturnsPaymentWithId()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 100
        };

        await m_uut.PostAsync(p1);

        Assert.DoesNotThrowAsync(async () => await m_uut.GetByIdAsync(p1.ID));
        var payment = await m_uut.GetByIdAsync(p1.ID);
        Assert.That(payment.ID, Is.EqualTo(p1.ID));
    }


    [Test]
    public async Task Get_GetNonExsistingPayment_ThrowsException()
    {
        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.GetByIdAsync(Guid.NewGuid()));
    }

    [Test]
    public async Task Put_UpdateExsistingPayment_UpdatedAttrMatches()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 100
        };

        await m_uut.PostAsync(p1);

        p1.PaymentAmount = 500;

        await m_uut.PutAsync(p1.ID, p1);
        var payment = await m_uut.GetByIdAsync(p1.ID);
        Assert.That(payment.TimeOfPayment, Is.EqualTo(p1.TimeOfPayment));
        Assert.That(payment.PaymentAmount, Is.EqualTo(p1.PaymentAmount));
        Assert.That(payment.PrePaymentBalance, Is.EqualTo(p1.PrePaymentBalance));
        Assert.That(payment.ID, Is.EqualTo(p1.ID));
        Assert.That(payment.UserID, Is.EqualTo(p1.UserID));
    }
    [TestCase(100)]
    [TestCase(1000)]
    [TestCase(500)]
    [TestCase(1)]
    public async Task Put_UpdateExsistingPaymentPayamountIncrease_UserSaldoIsUpdated(int saldoUpdate)
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 100
        };

        await m_uut.PostAsync(p1);
        var user = await m_userrepo.GetByIdAsync(p1.UserID);
        Assert.That(user.Saldo, Is.EqualTo(u1.Saldo + p1.PaymentAmount));

        p1.PaymentAmount = saldoUpdate;

        await m_uut.PutAsync(p1.ID, p1);
        user = await m_userrepo.GetByIdAsync(p1.UserID);
        Assert.That(user.Saldo, Is.EqualTo(u1.Saldo + saldoUpdate));
    }
    [Test]
    public async Task Put_UpdateExsistingPaymentWithNegativePaymentAmount_ThrowsException()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 100
        };

        await m_uut.PostAsync(p1);

        p1.PaymentAmount = -1;

        Assert.ThrowsAsync<WiseBet.backend.IRepository.InvalidParameterException>(async () => await m_uut.PutAsync(p1.ID, p1));
    }
    [Test]
    public async Task Put_UpdateNonExsistingPayment_ThrowsException()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 100
        };

        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.PutAsync(p1.ID, p1));
    }
    [Test]
    public async Task Put_UpdateExsistingPaymentSoUserSaldoGoesNegative_UserSaldoIsZero()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 1000
        };

        await m_uut.PostAsync(p1);

        // Decreaseing usersaldo
        var user = await m_userrepo.GetByIdAsync(p1.UserID);
        user.Saldo = 100;
        await m_userrepo.PutAsync(user.ID, user);

        p1.PaymentAmount = 100;

        await m_uut.PutAsync(p1.ID, p1);

        user = await m_userrepo.GetByIdAsync(p1.UserID);
        Assert.That(user.Saldo, Is.Zero);
        var payment = await m_uut.GetByIdAsync(p1.ID);
        Assert.That(payment.PaymentAmount, Is.EqualTo(100));
    }

    [Test]
    public async Task Delete_RemovingExsistingPayment_Success()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 1000
        };

        await m_uut.PostAsync(p1);

        Assert.DoesNotThrowAsync(async () => await m_uut.DeleteAsync(p1));
    }
    [Test]
    public async Task Delete_RemovingExsistingPaymentTwice_ThrowsException()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 1000
        };

        await m_uut.PostAsync(p1);

        Assert.DoesNotThrowAsync(async () => await m_uut.DeleteAsync(p1));
        Assert.ThrowsAsync<WiseBet.backend.IRepository.KeyNotFoundException>(async () => await m_uut.DeleteAsync(p1));
    }
    [Test]
    public async Task Get_GetAllPaymentsByExsistingUser_ReturnListWithPayments()
    {
        PaymentDto p1 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 100
        };
        PaymentDto p2 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u1.ID,
            PaymentAmount = 1000
        };
        PaymentDto p3 = new PaymentDto
        {
            ID = Guid.NewGuid(),
            UserID = u2.ID,
            PaymentAmount = 10
        };

        await m_uut.PostAsync(p1);
        await m_uut.PostAsync(p2);
        await m_uut.PostAsync(p3);

        var U1payments = await m_uut.GetAllChatByUser(u1.ID);
        var U2payments = await m_uut.GetAllChatByUser(u2.ID);
        Assert.That(U1payments.Count, Is.EqualTo(2));
        Assert.That(U2payments.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Get_GetAllPaymentsByExsistingUser_ReturnEmptyList()
    {
        var U1payments = await m_uut.GetAllChatByUser(u1.ID);
        Assert.That(U1payments.Count, Is.Zero);
    }
}
