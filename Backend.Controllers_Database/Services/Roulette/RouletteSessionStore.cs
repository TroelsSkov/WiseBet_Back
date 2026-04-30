namespace WiseBet.backend.Services.Roulette;

public class RouletteSessionStore
{
    private readonly object _lock = new();
    private readonly List<RouletteSessionState> _sessions = new();

    public RouletteSessionState? GetSessionForUser(Guid userId)
    {
        lock (_lock)
        {
            return _sessions.FirstOrDefault(s => s.Participants.Contains(userId));
        }
    }

    public RouletteSessionState? GetSessionWithCapacity(int maxUsersPerSession)
    {
        lock (_lock)
        {
            return _sessions.FirstOrDefault(s => s.Participants.Count < maxUsersPerSession);
        }
    }

    public RouletteSessionState CreateSession()
    {
        lock (_lock)
        {
            var session = new RouletteSessionState();
            _sessions.Add(session);
            return session;
        }
    }

    public void AddUserToSession(RouletteSessionState session, Guid userId)
    {
        lock (_lock)
        {
            session.Participants.Add(userId);
        }
    }

    public void RemoveUserFromSession(RouletteSessionState session, Guid userId)
    {
        lock (_lock)
        {
            session.Participants.Remove(userId);
            session.Bets.RemoveAll(x => x.UserId == userId);
            if (session.Participants.Count == 0)
                _sessions.Remove(session);
        }
    }
}
