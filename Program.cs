using Microsoft.Extensions.Configuration;
using QuizletBot.Handlers;
using QuizletBot.Models;
using QuizletBot.Resources;
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

var repositories = Decks.All.ToDictionary(
    deck => deck.Id,
    deck => new PhraseologismRepository(Path.Combine(baseDir, "Data", deck.DataFile)));

var imagesDir = Path.Combine(baseDir, "Assets", "Images");
if (!Directory.Exists(imagesDir))
{
    Console.WriteLine($"❌ Images folder not found: {imagesDir}");
    Console.WriteLine("Make sure Assets/Images/*.png is next to the built exe.");
    return;
}

var fontPath = Path.Combine(baseDir, "Assets", "Fonts", "DejaVuSans-Bold.ttf");
if (!File.Exists(fontPath))
{
    Console.WriteLine($"❌ Font not found: {fontPath}");
    Console.WriteLine("Make sure Assets/Fonts/DejaVuSans-Bold.ttf is next to the built exe.");
    return;
}

var imageComposer = new CardImageComposer(fontPath);
var sessions = new SessionManager();
var userDataStore = new UserDataStore(Path.Combine(baseDir, "userdata.json"));
var handler = new UpdateHandler(repositories, sessions, userDataStore, imagesDir, imageComposer);

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
Console.WriteLine($"✅ NMT Learner (@{me.Username}) запущено.");
foreach (var deck in Decks.All)
{
    Console.WriteLine($"   {deck.Emoji} {deck.Name(Language.UA)}: {repositories[deck.Id].Count} карток");
}
Console.WriteLine("Натисніть Ctrl+C, щоб зупинити.");

var exitEvent = new ManualResetEvent(false);
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    exitEvent.Set();
};
exitEvent.WaitOne();
cts.Cancel();
