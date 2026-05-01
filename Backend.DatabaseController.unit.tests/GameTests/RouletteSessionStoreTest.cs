using WiseBet.backend.Services.Roulette;

namespace Backend.DatabaseController.unit.tests.GameTests;

[TestFixture]
public class RouletteSessionStoreTest
{
    [Test]
    public void TryJoinExistingSession_WhenFull_ReturnsNull()
    {
        var store = new RouletteSessionStore();
        var firstUser = Guid.NewGuid();
        store.CreateNewSessionWithUser(firstUser);
        for (int i = 0; i < 4; i++)
            store.TryJoinExistingSession(Guid.NewGuid(), 5);

        var result = store.TryJoinExistingSession(Guid.NewGuid(), 5);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void TryJoinExistingSession_AfterFiveUsers_SixthMustUseNewSession()
    {
        var store = new RouletteSessionStore();
        var u0 = Guid.NewGuid();
        store.CreateNewSessionWithUser(u0);
        for (var i = 0; i < 4; i++)
            Assert.That(store.TryJoinExistingSession(Guid.NewGuid(), 5), Is.Not.Null);

        var first = store.GetSessionForUser(u0);
        Assert.That(first, Is.Not.Null);
        Assert.That(first!.Participants.Count, Is.EqualTo(5));
        Assert.That(store.TryJoinExistingSession(Guid.NewGuid(), 5), Is.Null);

        var u6 = Guid.NewGuid();
        var second = store.CreateNewSessionWithUser(u6);
        Assert.That(second.Participants.Count, Is.EqualTo(1));
        Assert.That(store.GetSessionForUser(u6), Is.SameAs(second));
    }

    [Test]
    public void RemoveUserFromSession_WhenLastUser_RemovesSession()
    {
        var store = new RouletteSessionStore();
        var userId = Guid.NewGuid();
        var session = store.CreateNewSessionWithUser(userId);

        store.RemoveUserFromSession(session, userId);
        var sessionLookup = store.GetSessionForUser(userId);

        Assert.That(sessionLookup, Is.Null);
    }
}
