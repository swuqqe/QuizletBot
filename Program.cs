using Microsoft.Extensions.Configuration;
using QuizletBot.Handlers;
using QuizletBot.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

var baseDir = AppContext.BaseDirectory;

// Token comes from TELEGRAM_BOT_TOKEN, falling back to appsettings.json ("BotToken").
var config = new ConfigurationBuilder()
    .SetBasePath(baseDir)
    .AddJsonFile("appsettings.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var token = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN") ?? config["BotToken"];

if (string.IsNullOrWhiteSpace(token) || token.Contains("YOUR_BOT_TOKEN"))
{
    Console.WriteLine("❌ Не заданий токен бота.");
    Console.WriteLine("Встановіть змінну середовища TELEGRAM_BOT_TOKEN або впишіть токен у appsettings.json.");
    return;
}

var repository = new PhraseologismRepository(Path.Combine(baseDir, "Data", "phraseologisms.json"));
var sessions = new SessionManager();
var userDataStore = new UserDataStore(Path.Combine(baseDir, "userdata.json"));
var handler = new UpdateHandler(repository, sessions, userDataStore);

var botClient = new TelegramBotClient(token);
using var cts = new CancellationTokenSource();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery }
};

botClient.StartReceiving(
    updateHandler: handler.HandleUpdateAsync,
    pollingErrorHandler: handler.HandlePollingErrorAsync,
    receiverOptions: receiverOptions,
    cancellationToken: cts.Token);

var me = await botClient.GetMeAsync(cts.Token);
Console.WriteLine($"✅ Бот @{me.Username} запущено. Картотека: {repository.Count} фразеологізмів.");
Console.WriteLine("Натисніть Ctrl+C, щоб зупинити.");

var exitEvent = new ManualResetEvent(false);
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    exitEvent.Set();
};
exitEvent.WaitOne();
cts.Cancel();
