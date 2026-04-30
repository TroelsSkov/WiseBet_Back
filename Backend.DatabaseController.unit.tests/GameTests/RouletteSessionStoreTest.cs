using WiseBet.backend.Services.Roulette;

namespace Backend.DatabaseController.unit.tests.GameTests;

[TestFixture]
public class RouletteSessionStoreTest
{
    [Test]
    public void GetSessionWithCapacity_WhenFull_ReturnsNull()
    {
        var store = new RouletteSessionStore();
        var session = store.CreateSession();
        for (int i = 0; i < 5; i++)
            store.AddUserToSession(session, Guid.NewGuid());

        var result = store.GetSessionWithCapacity(5);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void RemoveUserFromSession_WhenLastUser_RemovesSession()
    {
        var store = new RouletteSessionStore();
        var userId = Guid.NewGuid();
        var session = store.CreateSession();
        store.AddUserToSession(session, userId);

        store.RemoveUserFromSession(session, userId);
        var sessionLookup = store.GetSessionForUser(userId);

        Assert.That(sessionLookup, Is.Null);
    }
}
