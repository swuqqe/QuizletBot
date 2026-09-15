namespace QuizletBot.Models;

// Stuff that needs to survive a bot restart: language and all-time stats.
public class UserData
{
    public long ChatId { get; set; }
    public Language Language { get; set; } = Language.UA;

    public int TotalReviewed { get; set; }
    public int TotalKnown { get; set; }
    public int TotalUnknown { get; set; }
}
