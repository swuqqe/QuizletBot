namespace QuizletBot.Models;

// Where the user currently is, used to route callbacks correctly.
public enum SessionState
{
    Idle,
    ChoosingMode,
    ChoosingDeck,
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

    // Which mode and deck the current/last session used - kept across
    // ResetSession so "Try Again" reuses the same setup.
    public GameMode CurrentMode { get; set; } = GameMode.Flip;
    public string CurrentDeckId { get; set; } = "idioms";

    // Current flashcard round
    public int SessionLimit { get; set; }
    public int SessionShown { get; set; }
    public int SessionKnown { get; set; }
    public int SessionUnknown { get; set; }

    // Count picked on the count screen but not confirmed with Start yet.
    public int PendingCount { get; set; }

    public int CurrentCardId { get; set; }
    public bool IsFlipped { get; set; }

    // Cards already shown this round, so we don't repeat them.
    public HashSet<int> ShownCardIds { get; set; } = new();

    // Choose-Correct mode: the 3 answer options currently on screen and
    // which one is right, so we can grade whichever button gets tapped.
    public List<string> CurrentChoices { get; set; } = new();
    public int CorrectChoiceIndex { get; set; }

    public void ResetSession(int limit)
    {
        State = SessionState.Active;
        SessionLimit = limit;
        PendingCount = 0;
        SessionShown = 0;
        SessionKnown = 0;
        SessionUnknown = 0;
        CurrentCardId = 0;
        IsFlipped = false;
        ShownCardIds.Clear();
        CurrentChoices.Clear();
        CorrectChoiceIndex = 0;
    }
}
