using QuizletBot.Models;
using QuizletBot.Resources;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizletBot.Services;

// Builds (text, keyboard) for every screen. Callback data uses
// "namespace:action[:param]" so the handler can route with one switch.
public static class UiRenderer
{
    private static readonly int[] SessionSizes = { 5, 15, 25, 50 };

    public static (string text, InlineKeyboardMarkup keyboard) MainMenu(Language l)
    {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnFlashcards(l), "nav:flashcards") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnStatistics(l), "nav:stats") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnSettings(l), "nav:settings") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnAbout(l), "nav:about") }
        });

        return (Strings.MainMenuTitle(l), keyboard);
    }

    public static (string text, InlineKeyboardMarkup keyboard) ChooseCount(Language l)
    {
        var countButtons = SessionSizes
            .Select(n => InlineKeyboardButton.WithCallbackData(n.ToString(), $"fc:count:{n}"))
            .ToArray();

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            countButtons,
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnBack(l), "nav:main") }
        });

        return (Strings.ChooseCountTitle(l), keyboard);
    }

    public static (string text, InlineKeyboardMarkup keyboard) CardFront(Language l, Phraseologism card, int shown, int limit)
    {
        var text = $"{Strings.SessionProgress(l, shown, limit)}\n\n" +
                    $"{Strings.CardFrontLabel(l)}\n\n<i>{Escape(card.Text)}</i>";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnShowExplanation(l), $"fc:flip:{card.Id}") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") }
        });

        return (text, keyboard);
    }

    public static (string text, InlineKeyboardMarkup keyboard) CardBack(Language l, Phraseologism card, int shown, int limit)
    {
        var text = $"{Strings.SessionProgress(l, shown, limit)}\n\n" +
                    $"{Strings.CardFrontLabel(l)}\n<i>{Escape(card.Text)}</i>\n\n" +
                    $"{Strings.CardExplanationLabel(l)}\n{Escape(card.Explanation)}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(Strings.BtnDontKnow(l), $"fc:dontknow:{card.Id}"),
                InlineKeyboardButton.WithCallbackData(Strings.BtnKnow(l), $"fc:know:{card.Id}")
            },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") }
        });

        return (text, keyboard);
    }

    public static (string text, InlineKeyboardMarkup keyboard) SessionEnd(Language l, int limit, int known, int unknown)
    {
        var text = $"{Strings.SessionEndTitle(l)}\n\n{Strings.SessionEndSummary(l, known + unknown, known, unknown)}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnTryAgain(l), $"fc:count:{limit}") },
            new[] { InlineKeyboardButton.WithCallbackData(Strings.BtnMainMenu(l), "nav:main") }
        });

        return (text, keyboard);
    }

    public static (string text, InlineKeyboardMarkup keyboard) Stats(Language l, UserData data)
    {
        var text = $"{Strings.StatsTitle(l)}\n\n{Strings.StatsBody(l, data.TotalReviewed, data.TotalKnown, data.TotalUnknown)}";
        var keyboard = NavRow(l);
        return (text, keyboard);
    }

    public static (string text, InlineKeyboardMarkup keyboard) Settings(Language l)
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

        return (text, keyboard);
    }

    public static (string text, InlineKeyboardMarkup keyboard) About(Language l)
    {
        var text = $"{Strings.AboutTitle(l)}\n\n{Strings.AboutBody(l)}";
        var keyboard = NavRow(l);
        return (text, keyboard);
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

    private static string Escape(string s) =>
        s.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
}
