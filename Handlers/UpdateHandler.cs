using QuizletBot.Models;
using QuizletBot.Resources;
using QuizletBot.Services;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace QuizletBot.Handlers;

// Routes every Telegram update. Callback data always follows
// "namespace:action[:param]" (e.g. "nav:main", "fc:know:42", "settings:lang:EN"),
// so we can figure out which screen to show next with a single switch,
// and nothing ever ends up stuck with no way forward.
public class UpdateHandler
{
    private readonly PhraseologismRepository _repository;
    private readonly SessionManager _sessions;
    private readonly UserDataStore _userData;

    public UpdateHandler(PhraseologismRepository repository, SessionManager sessions, UserDataStore userData)
    {
        _repository = repository;
        _sessions = sessions;
        _userData = userData;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message when update.Message?.Text is not null:
                    await HandleMessageAsync(bot, update.Message, ct);
                    break;

                case UpdateType.CallbackQuery when update.CallbackQuery is not null:
                    await HandleCallbackAsync(bot, update.CallbackQuery, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Помилка обробки апдейту: {ex}");
        }
    }

    public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken ct)
    {
        var msg = exception switch
        {
            ApiRequestException apiEx => $"Telegram API Error [{apiEx.ErrorCode}]: {apiEx.Message}",
            _ => exception.ToString()
        };
        Console.WriteLine($"⚠️ Помилка polling: {msg}");
        return Task.CompletedTask;
    }

    // ==================== Messages / commands ====================

    private async Task HandleMessageAsync(ITelegramBotClient bot, Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var text = message.Text!.Trim();
        var user = _userData.Get(chatId);
        var session = _sessions.GetOrCreate(chatId);

        if (text is "/start" or "/menu")
        {
            session.State = SessionState.Idle;
            await SendMainMenuAsync(bot, chatId, user.Language, ct, greet: text == "/start");
            return;
        }

        // Anything else - just show the main menu instead of leaving them stuck.
        await SendMainMenuAsync(bot, chatId, user.Language, ct);
    }

    // ==================== Callback buttons ====================

    private async Task HandleCallbackAsync(ITelegramBotClient bot, CallbackQuery callback, CancellationToken ct)
    {
        if (callback.Data is null || callback.Message is null) return;

        var chatId = callback.Message.Chat.Id;
        var messageId = callback.Message.MessageId;
        var user = _userData.Get(chatId);
        var session = _sessions.GetOrCreate(chatId);
        session.MessageId = messageId;

        var parts = callback.Data.Split(':');
        var ns = parts[0];
        var action = parts.Length > 1 ? parts[1] : "";
        var param = parts.Length > 2 ? parts[2] : "";

        switch (ns)
        {
            case "nav":
                await HandleNavAsync(bot, chatId, messageId, action, user, session, ct);
                break;

            case "fc":
                await HandleFlashcardsAsync(bot, chatId, messageId, action, param, user, session, ct);
                break;

            case "settings":
                await HandleSettingsAsync(bot, chatId, messageId, action, param, user, ct);
                break;
        }

        await bot.AnswerCallbackQueryAsync(callback.Id, cancellationToken: ct);
    }

    // ---------- nav:* - static screen navigation ----------
    private async Task HandleNavAsync(ITelegramBotClient bot, long chatId, int messageId, string action,
        UserData user, UserSession session, CancellationToken ct)
    {
        session.State = SessionState.Idle;

        var (text, keyboard) = action switch
        {
            "main" => UiRenderer.MainMenu(user.Language),
            "flashcards" => EnterChooseCount(session, user.Language),
            "stats" => UiRenderer.Stats(user.Language, user),
            "settings" => UiRenderer.Settings(user.Language),
            "about" => UiRenderer.About(user.Language),
            _ => UiRenderer.MainMenu(user.Language)
        };

        await EditAsync(bot, chatId, messageId, text, keyboard, ct);
    }

    private (string text, InlineKeyboardMarkup keyboard) EnterChooseCount(UserSession session, Language lang)
    {
        session.State = SessionState.ChoosingCount;
        return UiRenderer.ChooseCount(lang);
    }

    // ---------- fc:* - flashcard session ----------
    private async Task HandleFlashcardsAsync(ITelegramBotClient bot, long chatId, int messageId, string action,
        string param, UserData user, UserSession session, CancellationToken ct)
    {
        switch (action)
        {
            case "count":
            {
                var limit = int.TryParse(param, out var n) ? n : 5;
                session.ResetSession(limit);
                await ShowNextCardAsync(bot, chatId, messageId, user, session, ct);
                break;
            }

            case "flip":
            {
                if (session.State != SessionState.Active) break; // stale callback, ignore it
                var card = _repository.GetById(session.CurrentCardId);
                session.IsFlipped = true;
                var (text, keyboard) = UiRenderer.CardBack(user.Language, card, session.SessionShown, session.SessionLimit);
                await EditAsync(bot, chatId, messageId, text, keyboard, ct);
                break;
            }

            case "know":
            case "dontknow":
            {
                if (session.State != SessionState.Active) break;

                var known = action == "know";
                session.SessionKnown += known ? 1 : 0;
                session.SessionUnknown += known ? 0 : 1;

                user.TotalReviewed += 1;
                user.TotalKnown += known ? 1 : 0;
                user.TotalUnknown += known ? 0 : 1;
                _userData.Save(user);

                if (session.SessionShown >= session.SessionLimit)
                {
                    session.State = SessionState.Ended;
                    var (text, keyboard) = UiRenderer.SessionEnd(user.Language, session.SessionLimit, session.SessionKnown, session.SessionUnknown);
                    await EditAsync(bot, chatId, messageId, text, keyboard, ct);
                }
                else
                {
                    await ShowNextCardAsync(bot, chatId, messageId, user, session, ct);
                }
                break;
            }
        }
    }

    private async Task ShowNextCardAsync(ITelegramBotClient bot, long chatId, int messageId,
        UserData user, UserSession session, CancellationToken ct)
    {
        var card = _repository.GetRandom(session.ShownCardIds);
        session.CurrentCardId = card.Id;
        session.ShownCardIds.Add(card.Id);
        session.SessionShown += 1;
        session.IsFlipped = false;
        session.State = SessionState.Active;

        var (text, keyboard) = UiRenderer.CardFront(user.Language, card, session.SessionShown, session.SessionLimit);
        await EditAsync(bot, chatId, messageId, text, keyboard, ct);
    }

    // ---------- settings:* - language switch ----------
    private async Task HandleSettingsAsync(ITelegramBotClient bot, long chatId, int messageId, string action,
        string param, UserData user, CancellationToken ct)
    {
        if (action == "lang" && Enum.TryParse<Language>(param, out var lang))
        {
            user.Language = lang;
            _userData.Save(user);
        }

        var (text, keyboard) = UiRenderer.Settings(user.Language);
        await EditAsync(bot, chatId, messageId, text, keyboard, ct);
    }

    // ==================== Helpers ====================

    private async Task SendMainMenuAsync(ITelegramBotClient bot, long chatId, Language lang, CancellationToken ct, bool greet = false)
    {
        if (greet)
        {
            await bot.SendTextMessageAsync(chatId, Strings.WelcomeText(lang), cancellationToken: ct);
        }

        var (text, keyboard) = UiRenderer.MainMenu(lang);
        var sent = await bot.SendTextMessageAsync(chatId, text,
            parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);

        _sessions.GetOrCreate(chatId).MessageId = sent.MessageId;
    }

    private static async Task EditAsync(ITelegramBotClient bot, long chatId, int messageId,
        string text, InlineKeyboardMarkup keyboard, CancellationToken ct)
    {
        await bot.EditMessageTextAsync(chatId, messageId, text,
            parseMode: ParseMode.Html, replyMarkup: keyboard, cancellationToken: ct);
    }
}
