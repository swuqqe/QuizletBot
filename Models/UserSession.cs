namespace QuizletBot.Models;

// Where the user currently is, used to route callbacks correctly.
public enum SessionState
{
    Idle,
    ChoosingCount,
    Active,
    Ended
}

// In-memory session state per chat. Long-term stats live in UserData instead.
public class UserSession
{
    public SessionState State { get; set; } = SessionState.Idle;

    // Message we keep editing instead of sending new ones every time.
    public int MessageId { get; set; }

    // Current flashcard round
    public int SessionLimit { get; set; }
    public int SessionShown { get; set; }
    public int SessionKnown { get; set; }
    public int SessionUnknown { get; set; }

    public int CurrentCardId { get; set; }
    public bool IsFlipped { get; set; }

    // Cards already shown this round, so we don't repeat them.
    public HashSet<int> ShownCardIds { get; set; } = new();

    public void ResetSession(int limit)
    {
        State = SessionState.Active;
        SessionLimit = limit;
        SessionShown = 0;
        SessionKnown = 0;
        SessionUnknown = 0;
        CurrentCardId = 0;
        IsFlipped = false;
        ShownCardIds.Clear();
    }
}
