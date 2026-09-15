namespace QuizletBot.Models;

// One flashcard: a phrase and its explanation.
public class Phraseologism
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
}
