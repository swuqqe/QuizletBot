using System.Collections.Concurrent;
using QuizletBot.Models;

namespace QuizletBot.Services;

// Keeps each user's session in memory (chatId -> UserSession).
// Fine for a bot this size; swap for Redis/DB if it ever needs to scale.
public class SessionManager
{
    private readonly ConcurrentDictionary<long, UserSession> _sessions = new();

    public UserSession GetOrCreate(long chatId) =>
        _sessions.GetOrAdd(chatId, _ => new UserSession());

    public void Reset(long chatId) =>
        _sessions.TryRemove(chatId, out _);
}
