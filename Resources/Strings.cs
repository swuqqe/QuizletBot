using QuizletBot.Models;

namespace QuizletBot.Resources;

// All UI text lives here. Each method takes a Language and returns the right
// string - adding a language later just means adding a case to each method,
// instead of hunting for hardcoded text all over the codebase.
public static class Strings
{
    // ---------- Main menu ----------
    public static string MainMenuTitle(Language l) => l == Language.UA
        ? "🏠 <b>NMT Learner — Головне меню</b>\n\nОберіть розділ:"
        : "🏠 <b>NMT Learner — Main Menu</b>\n\nChoose a section:";

    public static string BtnFlashcards(Language l) => l == Language.UA ? "🚀 Розпочати" : "🚀 Start";
    public static string BtnStatistics(Language l) => l == Language.UA ? "📊 Статистика" : "📊 Statistics";
    public static string BtnSettings(Language l) => l == Language.UA ? "⚙️ Налаштування" : "⚙️ Settings";
    public static string BtnAbout(Language l) => l == Language.UA ? "ℹ️ Про бота" : "ℹ️ About Bot";

    // ---------- Shared navigation ----------
    public static string BtnBack(Language l) => l == Language.UA ? "◀️ Назад" : "◀️ Back";
    public static string BtnMainMenu(Language l) => l == Language.UA ? "🏠 Головне меню" : "🏠 Main Menu";

    // ---------- Mode picker ----------
    public static string ChooseModeTitle(Language l) => l == Language.UA
        ? "🎴 <b>Картки</b>\n\nОберіть режим:"
        : "🎴 <b>Flashcards</b>\n\nChoose a mode:";

    public static string BtnModeFlip(Language l) => l == Language.UA ? "🔄 Флеш-картки" : "🔄 Flip Cards";
    public static string BtnModeChoose(Language l) => l == Language.UA ? "❓ Вибери правильну" : "❓ Choose Correct";

    // ---------- Deck picker ("What to study") ----------
    public static string ChooseDeckTitle(Language l) => l == Language.UA
        ? "🎴 <b>Що вивчаємо?</b>\n\nОберіть набір карток:"
        : "🎴 <b>What to study?</b>\n\nChoose a card set:";

    // ---------- Card count picker ----------
    public static string ChooseCountTitle(Language l) => l == Language.UA
        ? "🎴 <b>Картки</b>\n\nСкільки карток хочете пройти цього разу?"
        : "🎴 <b>Flashcards</b>\n\nHow many cards would you like to review?";

    public static string ChooseCountSelected(Language l, int count) => l == Language.UA
        ? $"Обрано: <b>{count}</b> карток. Натисніть «Старт», щоб почати."
        : $"Selected: <b>{count}</b> cards. Tap Start to begin.";

    public static string BtnStart(Language l) => l == Language.UA ? "🚀 Старт" : "🚀 Start";

    // ---------- Card front/back (Flip mode) ----------
    public static string BtnShowExplanation(Language l) => l == Language.UA ? "🔄 Показати пояснення" : "🔄 Show explanation";
    public static string BtnKnow(Language l) => l == Language.UA ? "➡️ Знаю" : "➡️ Know";
    public static string BtnDontKnow(Language l) => l == Language.UA ? "⬅️ Не знаю" : "⬅️ Don't know";

    public static string SessionProgress(Language l, int shown, int limit) => l == Language.UA
        ? $"Картка {shown} з {limit}"
        : $"Card {shown} of {limit}";

    // ---------- Choose-Correct mode ----------
    public static string ChooseCorrectResultTitle(Language l, bool correct) => l == Language.UA
        ? (correct ? "✅ Правильно!" : "❌ Неправильно")
        : (correct ? "✅ Correct!" : "❌ Wrong");

    public static string ChooseCorrectAnswerReveal(Language l, string correctAnswer) => l == Language.UA
        ? $"Правильна відповідь: <b>{correctAnswer}</b>"
        : $"Correct answer: <b>{correctAnswer}</b>";

    public static string BtnNext(Language l) => l == Language.UA ? "➡️ Далі" : "➡️ Next";

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

    public static string StatsCombinedTotal(Language l, int total) => l == Language.UA
        ? $"Усього переглянуто карток: <b>{total}</b>"
        : $"Total cards reviewed: <b>{total}</b>";

    public static string StatsModeSection(Language l, string sectionName, int totalReviewed, int totalKnown, int totalUnknown) => l == Language.UA
        ? $"<b>{sectionName}</b>\nПереглянуто: <b>{totalReviewed}</b> | ➡️ Знаю: <b>{totalKnown}</b> | ⬅️ Не знаю: <b>{totalUnknown}</b>"
        : $"<b>{sectionName}</b>\nReviewed: <b>{totalReviewed}</b> | ➡️ Know: <b>{totalKnown}</b> | ⬅️ Don't know: <b>{totalUnknown}</b>";

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
        ? "<b>NMT Learner Bot</b> — тренажер для підготовки до НМТ: фразеологізми, наголоси та лексичні помилки у форматі флеш-карток і тестів.\n\n" +
          "🛠 <b>Технології:</b> C#, .NET 8, Telegram.Bot\n\n" +
          "👤 <b>Розробник:</b> @yfsja7777\n\n" +
          "📂 <b>Відкритий код:</b> https://github.com/swuqqe/QuizletBot"
        : "<b>NMT Learner Bot</b> — an NMT exam prep trainer: idioms, word stress, and lexical mistakes as flashcards and quizzes.\n\n" +
          "🛠 <b>Tech stack:</b> C#, .NET 8, Telegram.Bot\n\n" +
          "👤 <b>Developer:</b> @yfsja7777\n\n" +
          "📂 <b>Open-source code:</b> https://github.com/swuqqe/QuizletBot";
          

    // ---------- Greeting ----------
    public static string WelcomeText(Language l) => l == Language.UA
        ? "Привіт! Я NMT Learner — бот-тренажер для підготовки до НМТ 👋"
        : "Hi! I'm NMT Learner, your NMT exam prep bot 👋";
}
