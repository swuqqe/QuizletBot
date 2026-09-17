namespace QuizletBot.Models;

// One deck's running totals.
public class ModeStats
{
    public int TotalReviewed { get; set; }
    public int TotalKnown { get; set; }
    public int TotalUnknown { get; set; }
}

// Stuff that needs to survive a bot restart: language and all-time stats,
// tracked per deck (idioms / stress / lexical) - this is what actually tells
// you what you've learned, unlike splitting by game mode.
public class UserData
{
    public long ChatId { get; set; }
    public Language Language { get; set; } = Language.UA;

    public Dictionary<string, ModeStats> DeckStats { get; set; } = new();

    public ModeStats StatsFor(string deckId)
    {
        if (!DeckStats.TryGetValue(deckId, out var stats))
        {
            stats = new ModeStats();
            DeckStats[deckId] = stats;
        }
        return stats;
    }
}
