using QuizletBot.Models;
using QuizletBot.Resources;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizletBot.Services;

// Builds (text, keyboard, image file) for every screen. Callback data uses
// "namespace:action[:param]" so the handler can route with one switch.
public static class UiRenderer
{
    private static readonly int[] SessionSizes = { 5, 15, 25, 50 };
    private const int MaxChoiceLabelLength = 55;

    public static (string text, InlineKeyboardMarkup keyboard, string image) MainMenu(Language l)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnFlashcards(l), "nav:flashcards") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnStatistics(l), "nav:stats") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnSettings(l), "nav:settings") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnAbout(l), "nav:about") }
        });

        return (Strings.MainMenuTitle(l), keyboard, ScreenImages.MainMenu);
    }

    public static (string text, InlineKeyboardMarkup keyboard, string image) ChooseMode(Language l)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnModeFlip(l), "mode:choose:flip") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnModeChoose(l), "mode:choose:choose") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnBack(l), "nav:main") }
        });

        return (Strings.ChooseModeTitle(l), keyboard, ScreenImages.ModePicker);
    }

    public static (string text, InlineKeyboardMarkup keyboard, string image) ChooseDeck(Language l)
    {
        var deckButtons = Decks.All
            .Select(d => new[] { InlineKeyboardButton.WithCallbackData($"{d.Emoji} {d.Name(l)}", $"deck:choose:{d.Id}") })
            .ToArray();

        var keyboard = new InlineKeyboardMarkup(deckButtons.Append(
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnBack(l), "nav:main") }
        ));

        return (Strings.ChooseDeckTitle(l), keyboard, ScreenImages.DeckPicker);
    }

    public static (string text, InlineKeyboardMarkup keyboard, string image) ChooseCount(Language l, DeckDefinition deck, int selectedCount)
    {
        var countButtons = SessionSizes
            .Select(n => InlineKeyboardButton.WithCallbackData(
                n == selectedCount ? $"✅ {n}" : n.ToString(), $"fc:setcount:{n}"))
            .ToArray();

        var rows = new List<InlineKeyboardButton[]> { countButtons };

        if (selectedCount > 0)
        {
            rows.Add(new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnStart(l), "fc:start") });
        }

        rows.Add(new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnBack(l), "nav:main") });

        var title = Strings.ChooseCountTitle(l);
        var text = selectedCount > 0
            ? $"{title}\n\n{Strings.ChooseCountSelected(l, selectedCount)}"
            : title;

        return (text, new InlineKeyboardMarkup(rows), deck.ImageFile);
    }

    // ---------- Flip mode ----------

    public static (string text, InlineKeyboardMarkup keyboard, string image) CardFront(Language l, DeckDefinition deck, Phraseologism card, int shown, int limit)
    {
        var text = $"{Strings.SessionProgress(l, shown, limit)}\n\n" +
                    $"{deck.FrontLabel(l)}\n\n<i>{Escape(card.Text)}</i>";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnShowExplanation(l), $"fc:flip:{card.Id}") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") }
        });

        return (text, keyboard, deck.ImageFile);
    }

    public static (string text, InlineKeyboardMarkup keyboard, string image) CardBack(Language l, DeckDefinition deck, Phraseologism card, int shown, int limit)
    {
        var text = $"{Strings.SessionProgress(l, shown, limit)}\n\n" +
                    $"{deck.FrontLabel(l)}\n<i>{Escape(card.Text)}</i>\n\n" +
                    $"{deck.BackLabel(l)}\n{Escape(card.Explanation)}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Strings.BtnDontKnow(l), $"fc:dontknow:{card.Id}"),
                InlineKeyboardButton.WithCallbackData(Strings.BtnKnow(l), $"fc:know:{card.Id}")
            },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") }
        });

        return (text, keyboard, deck.ImageFile);
    }

    // ---------- Choose-Correct mode ----------

    public static (string text, InlineKeyboardMarkup keyboard, string image) ChooseCorrectQuestion(
        Language l, DeckDefinition deck, Phraseologism card, List<string> choices, int shown, int limit)
    {
        var text = $"{Strings.SessionProgress(l, shown, limit)}\n\n" +
                    $"{deck.QuizPrompt(l)}\n\n\u201c<b>{Escape(card.Text)}</b>\u201d";

        var numberEmoji = new[] { "1️⃣", "2️⃣", "3️⃣" };
        var rows = choices.Select((choice, i) => new[]
        {
            InlineKeyboardButton.WithCallbackData($"{numberEmoji[i]} {Truncate(choice)}", $"fc:pick:{i}")
        }).ToList();
        rows.Add(new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") });

        return (text, new InlineKeyboardMarkup(rows), deck.ImageFile);
    }

    public static (string text, InlineKeyboardMarkup keyboard, string image) ChooseCorrectResult(
        Language l, DeckDefinition deck, Phraseologism card, bool wasCorrect, int shown, int limit)
    {
        var text = $"{Strings.SessionProgress(l, shown, limit)}\n\n" +
                    $"{Strings.ChooseCorrectResultTitle(l, wasCorrect)}\n\n" +
                    $"{deck.QuizPrompt(l)}\n\u201c<b>{Escape(card.Text)}</b>\u201d\n\n" +
                    $"{Strings.ChooseCorrectAnswerReveal(l, Escape(card.Explanation))}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnNext(l), "fc:next") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") }
        });

        return (text, keyboard, deck.ImageFile);
    }

    // ---------- Session summary ----------

    public static (string text, InlineKeyboardMarkup keyboard, string image) SessionEnd(Language l, int limit, int known, int unknown)
    {
        var text = $"{Strings.SessionEndTitle(l)}\n\n{Strings.SessionEndSummary(l, known + unknown, known, unknown)}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnTryAgain(l), $"fc:count:{limit}") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") }
        });

        return (text, keyboard, ScreenImages.SessionEnd);
    }

    // ---------- Statistics ----------

    public static (string text, InlineKeyboardMarkup keyboard, string image) Stats(Language l, UserData data)
    {
        var sections = Decks.All.Select(deck =>
        {
            var s = data.StatsFor(deck.Id);
            return Strings.StatsModeSection(l, $"{deck.Emoji} {deck.Name(l)}", s.TotalReviewed, s.TotalKnown, s.TotalUnknown);
        });

        var combined = Decks.All.Sum(deck => data.StatsFor(deck.Id).TotalReviewed);

        var text = $"{Strings.StatsTitle(l)}\n\n{Strings.StatsCombinedTotal(l, combined)}\n\n{string.Join("\n\n", sections)}";
        return (text, NavRow(l), ScreenImages.Stats);
    }

    // ---------- Settings ----------

    public static (string text, InlineKeyboardMarkup keyboard, string image) Settings(Language l)
    {
        var text = $"{Strings.SettingsTitle(l)}\n\n{Strings.SettingsCurrentLanguage(l)}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🇺🇦 UA", "settings:lang:UA"),
                InlineKeyboardButton.WithCallbackData("🇬🇧 EN", "settings:lang:EN")
            },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnBack(l), "nav:main") }
        });

        return (text, keyboard, ScreenImages.Settings);
    }

    // ---------- About ----------

    public static (string text, InlineKeyboardMarkup keyboard, string image) About(Language l)
    {
        var text = $"{Strings.AboutTitle(l)}\n\n{Strings.AboutBody(l)}";
        return (text, NavRow(l), ScreenImages.About);
    }

    // Back and Main Menu both just go to the main menu for now - these screens
    // are only one level deep. Fine to split them later if submenus get nested.
    private static InlineKeyboardMarkup NavRow(Language l) => new(new[]
    {
        new[]
        {
            InlineKeyboardButton.WithCallbackData(Strings.BtnBack(l), "nav:main"),
            InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main")
        }
    });

    private static string Truncate(string s) =>
        s.Length <= MaxChoiceLabelLength ? s : s[..MaxChoiceLabelLength] + "…";

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
