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

    public RouletteSessionState? TryJoinExistingSession(Guid userId, int maxUsersPerSession)
    {
        lock (_lock)
        {
            foreach (var s in _sessions)
            {
                if (s.Participants.Contains(userId))
                    return s;
            }

            foreach (var s in _sessions)
            {
                if (s.Participants.Count < maxUsersPerSession)
                {
                    s.Participants.Add(userId);
                    return s;
                }
            }

            return null;
        }
    }

    public RouletteSessionState CreateNewSessionWithUser(Guid userId)
    {
        lock (_lock)
        {
            foreach (var s in _sessions)
            {
                if (s.Participants.Contains(userId))
                    return s;
            }

            var session = new RouletteSessionState();
            session.Participants.Add(userId);
            _sessions.Add(session);
            return session;
        }
    }


    public void RemoveUserFromSession(RouletteSessionState session, Guid userId)
    {
        lock (_lock)
        {
            session.Participants.Remove(userId);
            if (session.Participants.Count == 0)
                _sessions.Remove(session);
        }
    }

    public bool IsLastParticipant(RouletteSessionState session, Guid userId)
    {
        lock (_lock)
        {
            return session.Participants.Count == 1 && session.Participants.Contains(userId);
        }
    }
    public IReadOnlyList<RouletteSessionState> SnapshotSessions()
    {
        lock (_lock)
        {
            return _sessions.ToArray();
        }
    }
}
