using QuizletBot.Models;
using QuizletBot.Resources;

namespace QuizletBot.Services;

// Builds the wrong answers for Choose-Correct mode. Most decks just pull
// other cards' explanations, but that makes no sense for the stress deck -
// "where's the stress in звисока" needs wrong answers that are the SAME
// word stressed differently, not unrelated words entirely.
public static class DistractorGenerator
{
    private const string Vowels = "аеєиіїоуюяАЕЄИІЇОУЮЯ";

    public static List<string> Generate(DeckDefinition deck, PhraseologismRepository repo, Phraseologism card, int count, Random rng)
    {
        if (deck.Id == "stress")
        {
            var variants = StressVariants(card.Explanation, count, rng);
            if (variants.Count < count)
            {
                // Word too short to fake enough alternate stress positions - pad with
                // real other words so the round still has `count` options.
                var filler = repo.GetRandomOthers(card.Id, count - variants.Count).Select(x => x.Explanation);
                variants.AddRange(filler);
            }
            return variants;
        }

        return repo.GetRandomOthers(card.Id, count).Select(x => x.Explanation).ToList();
    }

    // Takes the correctly-stressed word (one capitalized vowel) and produces
    // other versions of the same word with the capital moved to a different vowel.
    private static List<string> StressVariants(string correctForm, int count, Random rng)
    {
        var lower = correctForm.ToLowerInvariant();

        var correctIndex = -1;
        for (var i = 0; i < correctForm.Length; i++)
        {
            if (char.IsUpper(correctForm[i]))
            {
                correctIndex = i;
                break;
            }
        }

        var vowelIndices = new List<int>();
        for (var i = 0; i < lower.Length; i++)
        {
            if (Vowels.IndexOf(lower[i]) >= 0) vowelIndices.Add(i);
        }

        var otherIndices = vowelIndices
            .Where(i => i != correctIndex)
            .OrderBy(_ => rng.Next())
            .Take(count);

        return otherIndices
            .Select(i => lower[..i] + char.ToUpperInvariant(lower[i]) + lower[(i + 1)..])
            .ToList();
    }
}
