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
    private readonly Dictionary<string, PhraseologismRepository> _repositories;
    private readonly SessionManager _sessions;
    private readonly UserDataStore _userData;
    private readonly string _imagesDir;
    private readonly CardImageComposer _imageComposer;
    private static readonly Random Shuffler = new();

    public UpdateHandler(Dictionary<string, PhraseologismRepository> repositories, SessionManager sessions,
        UserDataStore userData, string imagesDir, CardImageComposer imageComposer)
    {
        _repositories = repositories;
        _sessions = sessions;
        _userData = userData;
        _imagesDir = imagesDir;
        _imageComposer = imageComposer;
    }

    private PhraseologismRepository RepoFor(UserSession session) => _repositories[session.CurrentDeckId];

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
            if (text == "/start")
            {
                await bot.SendTextMessageAsync(chatId, Strings.WelcomeText(user.Language), cancellationToken: ct);
            }
            await SendMainMenuAsync(bot, chatId, user.Language, ct);
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

            case "mode":
                await HandleModeAsync(bot, chatId, messageId, action, param, user, session, ct);
                break;

            case "deck":
                await HandleDeckAsync(bot, chatId, messageId, action, param, user, session, ct);
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

        var (text, keyboard, image) = action switch
        {
            "main" => UiRenderer.MainMenu(user.Language),
            "flashcards" => EnterChooseDeck(session, user.Language),
            "stats" => UiRenderer.Stats(user.Language, user),
            "settings" => UiRenderer.Settings(user.Language),
            "about" => UiRenderer.About(user.Language),
            _ => UiRenderer.MainMenu(user.Language)
        };

        await EditPhotoAsync(bot, chatId, messageId, text, keyboard, image, ct);
    }

    private (string text, InlineKeyboardMarkup keyboard, string image) EnterChooseDeck(UserSession session, Language lang)
    {
        session.State = SessionState.ChoosingDeck;
        return UiRenderer.ChooseDeck(lang);
    }

    // ---------- deck:* - card set picker ("what to study") ----------
    private async Task HandleDeckAsync(ITelegramBotClient bot, long chatId, int messageId, string action,
        string param, UserData user, UserSession session, CancellationToken ct)
    {
        if (action == "choose" && _repositories.ContainsKey(param))
        {
            session.CurrentDeckId = param;
            session.State = SessionState.ChoosingMode;
        }

        var (text, keyboard, image) = UiRenderer.ChooseMode(user.Language);
        await EditPhotoAsync(bot, chatId, messageId, text, keyboard, image, ct);
    }

    // ---------- mode:* - flip vs choose-correct picker ----------
    private async Task HandleModeAsync(ITelegramBotClient bot, long chatId, int messageId, string action,
        string param, UserData user, UserSession session, CancellationToken ct)
    {
        if (action == "choose")
        {
            session.CurrentMode = param == "choose" ? GameMode.ChooseCorrect : GameMode.Flip;
            session.State = SessionState.ChoosingCount;
            session.PendingCount = 0;
        }

        var deck = Decks.Get(session.CurrentDeckId);
        var (text, keyboard, image) = UiRenderer.ChooseCount(user.Language, deck, session.PendingCount);
        await EditPhotoAsync(bot, chatId, messageId, text, keyboard, image, ct);
    }

    // ---------- fc:* - flashcard / quiz session ----------
    private async Task HandleFlashcardsAsync(ITelegramBotClient bot, long chatId, int messageId, string action,
        string param, UserData user, UserSession session, CancellationToken ct)
    {
        switch (action)
        {
            case "setcount":
            {
                session.PendingCount = int.TryParse(param, out var picked) ? picked : 5;
                var deck = Decks.Get(session.CurrentDeckId);
                var (text, keyboard, image) = UiRenderer.ChooseCount(user.Language, deck, session.PendingCount);
                await EditPhotoAsync(bot, chatId, messageId, text, keyboard, image, ct);
                break;
            }

            case "start":
            {
                session.ResetSession(session.PendingCount > 0 ? session.PendingCount : 5);
                await ShowNextRoundAsync(bot, chatId, messageId, user, session, ct);
                break;
            }

            case "count": // used by "Try Again" on the summary screen - starts right away, same setup
            {
                var limit = int.TryParse(param, out var n) ? n : 5;
                session.ResetSession(limit);
                await ShowNextRoundAsync(bot, chatId, messageId, user, session, ct);
                break;
            }

            case "flip":
            {
                if (session.State != SessionState.Active) break; // stale callback, ignore it
                var deck = Decks.Get(session.CurrentDeckId);
                var card = RepoFor(session).GetById(session.CurrentCardId);
                session.IsFlipped = true;
                var (text, keyboard, _) = UiRenderer.CardBack(user.Language, deck, card, session.SessionShown, session.SessionLimit);
                await EditCardPhotoAsync(bot, chatId, messageId, text, keyboard, card.Explanation, CardStyle.Answer, ct);
                break;
            }

            case "know":
            case "dontknow":
            {
                if (session.State != SessionState.Active) break;
                RecordAnswer(session, user, known: action == "know");

                if (session.SessionShown >= session.SessionLimit)
                {
                    await ShowSessionEndAsync(bot, chatId, messageId, user, session, ct);
                }
                else
                {
                    await ShowNextRoundAsync(bot, chatId, messageId, user, session, ct);
                }
                break;
            }

            case "pick":
            {
                if (session.State != SessionState.Active) break;
                var pickedIndex = int.TryParse(param, out var i) ? i : -1;
                var wasCorrect = pickedIndex == session.CorrectChoiceIndex;
                RecordAnswer(session, user, known: wasCorrect);

                var deck = Decks.Get(session.CurrentDeckId);
                var card = RepoFor(session).GetById(session.CurrentCardId);
                var (text, keyboard, _) = UiRenderer.ChooseCorrectResult(user.Language, deck, card, wasCorrect, session.SessionShown, session.SessionLimit);
                var resultStyle = wasCorrect ? CardStyle.Correct : CardStyle.Incorrect;
                await EditCardPhotoAsync(bot, chatId, messageId, text, keyboard, card.Explanation, resultStyle, ct);
                break;
            }

            case "next":
            {
                if (session.SessionShown >= session.SessionLimit)
                {
                    await ShowSessionEndAsync(bot, chatId, messageId, user, session, ct);
                }
                else
                {
                    await ShowNextRoundAsync(bot, chatId, messageId, user, session, ct);
                }
                break;
            }
        }
    }

    private void RecordAnswer(UserSession session, UserData user, bool known)
    {
        session.SessionKnown += known ? 1 : 0;
        session.SessionUnknown += known ? 0 : 1;

        var stats = user.StatsFor(session.CurrentDeckId);
        stats.TotalReviewed += 1;
        stats.TotalKnown += known ? 1 : 0;
        stats.TotalUnknown += known ? 0 : 1;
        _userData.Save(user);
    }

    private async Task ShowNextRoundAsync(ITelegramBotClient bot, long chatId, int messageId,
        UserData user, UserSession session, CancellationToken ct)
    {
        var deck = Decks.Get(session.CurrentDeckId);
        var repo = RepoFor(session);
        var card = repo.GetRandom(session.ShownCardIds);
        session.CurrentCardId = card.Id;
        session.ShownCardIds.Add(card.Id);
        session.SessionShown += 1;
        session.IsFlipped = false;
        session.State = SessionState.Active;

        if (session.CurrentMode == GameMode.Flip)
        {
            var (text, keyboard, _) = UiRenderer.CardFront(user.Language, deck, card, session.SessionShown, session.SessionLimit);
            await EditCardPhotoAsync(bot, chatId, messageId, text, keyboard, card.Text, CardStyle.Question, ct);
        }
        else
        {
            var distractors = DistractorGenerator.Generate(deck, repo, card, 2, Shuffler);
            var choices = distractors.Append(card.Explanation).OrderBy(_ => Shuffler.Next()).ToList();
            session.CurrentChoices = choices;
            session.CorrectChoiceIndex = choices.IndexOf(card.Explanation);

            var (text, keyboard, _) = UiRenderer.ChooseCorrectQuestion(user.Language, deck, card, choices, session.SessionShown, session.SessionLimit);
            await EditCardPhotoAsync(bot, chatId, messageId, text, keyboard, card.Text, CardStyle.Question, ct);
        }
    }

    private async Task ShowSessionEndAsync(ITelegramBotClient bot, long chatId, int messageId, UserData user, UserSession session, CancellationToken ct)
    {
        session.State = SessionState.Ended;
        var (text, keyboard, image) = UiRenderer.SessionEnd(user.Language, session.SessionLimit, session.SessionKnown, session.SessionUnknown);
        await EditPhotoAsync(bot, chatId, messageId, text, keyboard, image, ct);
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

        var (text, keyboard, image) = UiRenderer.Settings(user.Language);
        await EditPhotoAsync(bot, chatId, messageId, text, keyboard, image, ct);
    }

    // ==================== Helpers ====================

    private async Task SendMainMenuAsync(ITelegramBotClient bot, long chatId, Language lang, CancellationToken ct)
    {
        var (text, keyboard, image) = UiRenderer.MainMenu(lang);
        var sent = await SendPhotoAsync(bot, chatId, text, keyboard, image, ct);
        _sessions.GetOrCreate(chatId).MessageId = sent.MessageId;
    }

    private async Task<Message> SendPhotoAsync(ITelegramBotClient bot, long chatId, string caption,
        InlineKeyboardMarkup keyboard, string imageFile, CancellationToken ct)
    {
        await using var stream = System.IO.File.OpenRead(Path.Combine(_imagesDir, imageFile));
        return await bot.SendPhotoAsync(
            chatId: chatId,
            photo: InputFile.FromStream(stream, imageFile),
            caption: caption,
            parseMode: ParseMode.Html,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task EditPhotoAsync(ITelegramBotClient bot, long chatId, int messageId,
        string caption, InlineKeyboardMarkup keyboard, string imageFile, CancellationToken ct)
    {
        await using var stream = System.IO.File.OpenRead(Path.Combine(_imagesDir, imageFile));
        var media = new InputMediaPhoto(InputFile.FromStream(stream, imageFile))
        {
            Caption = caption,
            ParseMode = ParseMode.Html
        };
        await bot.EditMessageMediaAsync(
            chatId: chatId,
            messageId: messageId,
            media: media,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    // Same as EditPhotoAsync, but renders the card's text as the full image -
    // composed on the fly instead of read from a static file. The style picks
    // the color + corner marker, so the picture visibly changes on flip/reveal.
    private async Task EditCardPhotoAsync(ITelegramBotClient bot, long chatId, int messageId,
        string caption, InlineKeyboardMarkup keyboard, string cardText, CardStyle style, CancellationToken ct)
    {
        var imageBytes = _imageComposer.Compose(cardText, style);
        using var stream = new MemoryStream(imageBytes);
        var media = new InputMediaPhoto(InputFile.FromStream(stream, "card.png"))
        {
            Caption = caption,
            ParseMode = ParseMode.Html
        };
        await bot.EditMessageMediaAsync(
            chatId: chatId,
            messageId: messageId,
            media: media,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }
}
