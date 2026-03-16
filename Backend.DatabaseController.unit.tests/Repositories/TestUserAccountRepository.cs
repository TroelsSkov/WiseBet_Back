using WiseBet.backend.IRepository;
using WiseBet.backend.Data;
using WiseBet.backend.DTOs;
namespace Backend.DatabaseController.unit.tests.Repositories;
public class Tests
{
    private UserAccountRepository m_uut;
    [SetUp]
    public void Setup()
    {
        m_uut = new UserAccountRepository(new DatabaseContext());
    }

    [Test]
    public void CheckTestsAreWorkingPassedAlways()
    {
        Assert.Pass();
    }
}
