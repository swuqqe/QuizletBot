namespace QuizletBot.Resources;

// Filenames (under Assets/Images/) for screens that aren't deck-specific.
// Deck banners live on DeckDefinition.ImageFile instead.
public static class ScreenImages
{
    public const string MainMenu = "main_menu.png";
    public const string ModePicker = "mode_picker.png";
    public const string DeckPicker = "deck_picker.png";
    public const string ChooseCount = "choose_count.png";
    public const string Stats = "stats.png";
    public const string Settings = "settings.png";
    public const string About = "about.png";

    // Session summary - picked by error rate, not a single fixed image.
    public const string SessionEndGood = "session_end_green.png";   // <=25% wrong
    public const string SessionEndOk = "session_end_yellow.png";    // 25-75% wrong
    public const string SessionEndBad = "session_end_red.png";      // >=75% wrong
}
