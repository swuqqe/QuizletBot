namespace QuizletBot.Models;

// One mode's running totals.
public class ModeStats
{
    public int TotalReviewed { get; set; }
    public int TotalKnown { get; set; }
    public int TotalUnknown { get; set; }
}

// Stuff that needs to survive a bot restart: language and all-time stats,
// tracked separately per game mode.
public class UserData
{
    public long ChatId { get; set; }
    public Language Language { get; set; } = Language.UA;

    public ModeStats Flip { get; set; } = new();
    public ModeStats ChooseCorrect { get; set; } = new();
}
