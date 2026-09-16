using QuizletBot.Models;

namespace QuizletBot.Resources;

// All UI text lives here. Each method takes a Language and returns the right
// string - adding a language later just means adding a case to each method,
// instead of hunting for hardcoded text all over the codebase.
public static class Strings
{
    // ---------- Main menu ----------
    public static string MainMenuTitle(Language l) => l == Language.UA
        ? "🏠 <b>Головне меню</b>\n\nОберіть розділ:"
        : "🏠 <b>Main Menu</b>\n\nChoose a section:";

    public static string BtnFlashcards(Language l) => l == Language.UA ? "🎴 Картки" : "🎴 Flashcards";
    public static string BtnStatistics(Language l) => l == Language.UA ? "📊 Статистика" : "📊 Statistics";
    public static string BtnSettings(Language l) => l == Language.UA ? "⚙️ Налаштування" : "⚙️ Settings";
    public static string BtnAbout(Language l) => l == Language.UA ? "ℹ️ Про бота" : "ℹ️ About Bot";

    // ---------- Shared navigation ----------
    public static string BtnBack(Language l) => l == Language.UA ? "◀️ Назад" : "◀️ Back";
    public static string BtnMainMenu(Language l) => l == Language.UA ? "🏠 Головне меню" : "🏠 Main Menu";

    // ---------- Card count picker ----------
    public static string ChooseDeckTitle(Language l) => l == Language.UA
        ? "🎴 <b>Картки</b>\n\nОберіть набір карток:"
        : "🎴 <b>Flashcards</b>\n\nChoose a card set:";

    public static string ChooseCountTitle(Language l) => l == Language.UA
        ? "🎴 <b>Картки</b>\n\nСкільки карток хочете пройти цього разу?"
        : "🎴 <b>Flashcards</b>\n\nHow many cards would you like to review?";

    // ---------- Card front/back ----------
    public static string BtnShowExplanation(Language l) => l == Language.UA ? "🔄 Показати пояснення" : "🔄 Show explanation";
    public static string BtnKnow(Language l) => l == Language.UA ? "➡️ Знаю" : "➡️ Know";
    public static string BtnDontKnow(Language l) => l == Language.UA ? "⬅️ Не знаю" : "⬅️ Don't know";

    public static string SessionProgress(Language l, int shown, int limit) => l == Language.UA
        ? $"Картка {shown} з {limit}"
        : $"Card {shown} of {limit}";

    // ---------- Session summary ----------
    public static string SessionEndTitle(Language l) => l == Language.UA
        ? "✅ <b>Сесію завершено!</b>"
        : "✅ <b>Session complete!</b>";

    public static string SessionEndSummary(Language l, int total, int known, int unknown) => l == Language.UA
        ? $"Переглянуто карток: <b>{total}</b>\n➡️ Знаю: <b>{known}</b>\n⬅️ Не знаю: <b>{unknown}</b>"
        : $"Cards reviewed: <b>{total}</b>\n➡️ Know: <b>{known}</b>\n⬅️ Don't know: <b>{unknown}</b>";

    public static string BtnTryAgain(Language l) => l == Language.UA ? "🔄 Спробувати ще раз" : "🔄 Try Again";

    // ---------- Statistics ----------
    public static string StatsTitle(Language l) => l == Language.UA
        ? "📊 <b>Ваша статистика</b>"
        : "📊 <b>Your Statistics</b>";

    public static string StatsBody(Language l, int totalReviewed, int totalKnown, int totalUnknown) => l == Language.UA
        ? $"Переглянуто карток за весь час: <b>{totalReviewed}</b>\n➡️ Знаю: <b>{totalKnown}</b>\n⬅️ Не знаю: <b>{totalUnknown}</b>"
        : $"Cards reviewed all-time: <b>{totalReviewed}</b>\n➡️ Know: <b>{totalKnown}</b>\n⬅️ Don't know: <b>{totalUnknown}</b>";

    // ---------- Settings ----------
    public static string SettingsTitle(Language l) => l == Language.UA
        ? "⚙️ <b>Налаштування</b>\n\nОберіть мову інтерфейсу:"
        : "⚙️ <b>Settings</b>\n\nChoose interface language:";

    public static string SettingsCurrentLanguage(Language l) => l == Language.UA
        ? "Поточна мова: 🇺🇦 Українська"
        : "Current language: 🇬🇧 English";

    // ---------- About ----------
    public static string AboutTitle(Language l) => l == Language.UA
        ? "ℹ️ <b>Про бота</b>"
        : "ℹ️ <b>About Bot</b>";

    public static string AboutBody(Language l) => l == Language.UA
        ? "QuizletBot — тренажер українських фразеологізмів у форматі флеш-карток.\n\n" +
          "🛠 <b>Технології:</b> C#, .NET 8, Telegram.Bot\n\n" +
          "👤 <b>Розробник:</b> @your_tag"
        : "QuizletBot — a flashcard trainer for Ukrainian idioms.\n\n" +
          "🛠 <b>Tech stack:</b> C#, .NET 8, Telegram.Bot\n\n" +
          "👤 <b>Developer:</b> @your_tag";

    // ---------- Greeting ----------
    public static string WelcomeText(Language l) => l == Language.UA
        ? "Привіт! Я бот-тренажер українських фразеологізмів 👋"
        : "Hi! I'm a Ukrainian idioms flashcard trainer 👋";
}
