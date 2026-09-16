using QuizletBot.Models;

namespace QuizletBot.Resources;

// One card set: where its data comes from, and how it should be labeled
// on screen. Front/back labels differ per deck since "phrase / explanation"
// doesn't quite fit "wrong word / correct stress" for example.
public class DeckDefinition
{
    public required string Id { get; init; }
    public required string DataFile { get; init; }
    public required string Emoji { get; init; }
    public required Func<Language, string> Name { get; init; }
    public required Func<Language, string> FrontLabel { get; init; }
    public required Func<Language, string> BackLabel { get; init; }
}

public static class Decks
{
    public static readonly DeckDefinition Idioms = new()
    {
        Id = "idioms",
        DataFile = "phraseologisms.json",
        Emoji = "📖",
        Name = l => l == Language.UA ? "Фразеологізми" : "Idioms",
        FrontLabel = l => l == Language.UA ? "📖 Фразеологізм:" : "📖 Phrase:",
        BackLabel = l => l == Language.UA ? "💡 Пояснення:" : "💡 Explanation:"
    };

    public static readonly DeckDefinition Stress = new()
    {
        Id = "stress",
        DataFile = "naholosy.json",
        Emoji = "🔤",
        Name = l => l == Language.UA ? "Наголоси" : "Word Stress",
        FrontLabel = l => l == Language.UA ? "🔤 Слово:" : "🔤 Word:",
        BackLabel = l => l == Language.UA ? "✅ Правильний наголос:" : "✅ Correct stress:"
    };

    public static readonly DeckDefinition LexicalMistakes = new()
    {
        Id = "lexical",
        DataFile = "leksychni_pomylky.json",
        Emoji = "✏️",
        Name = l => l == Language.UA ? "Лексичні помилки" : "Lexical Mistakes",
        FrontLabel = l => l == Language.UA ? "❌ Неправильно:" : "❌ Incorrect:",
        BackLabel = l => l == Language.UA ? "✅ Правильно:" : "✅ Correct:"
    };

    public static readonly DeckDefinition[] All = { Idioms, Stress, LexicalMistakes };

    public static DeckDefinition Get(string id) =>
        All.FirstOrDefault(d => d.Id == id) ?? Idioms;
}
