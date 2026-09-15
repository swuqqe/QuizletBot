using System.Text.Json;
using QuizletBot.Models;

namespace QuizletBot.Services;

// Loads the phrase list from JSON and hands out random cards.
public class PhraseologismRepository
{
    private readonly List<Phraseologism> _items;
    private readonly Random _random = new();

    public PhraseologismRepository(string jsonPath)
    {
        if (!File.Exists(jsonPath))
        {
            throw new FileNotFoundException(
                $"Can't find the phrase data file: {jsonPath}. " +
                "Make sure Data/phraseologisms.json is next to the built exe.");
        }

        var json = File.ReadAllText(jsonPath);
        _items = JsonSerializer.Deserialize<List<Phraseologism>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Phraseologism>();

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("phraseologisms.json is empty - add at least one entry.");
        }

        for (int i = 0; i < _items.Count; i++)
        {
            if (_items[i].Id == 0)
            {
                _items[i].Id = i + 1;
            }
        }
    }

    public int Count => _items.Count;

    public Phraseologism GetById(int id) =>
        _items.First(x => x.Id == id);

    // Returns a random card not in excludeIds (already shown this round).
    // If everything's excluded, just allow a repeat so the session doesn't get stuck.
    public Phraseologism GetRandom(HashSet<int>? excludeIds = null)
    {
        var pool = excludeIds is { Count: > 0 }
            ? _items.Where(x => !excludeIds.Contains(x.Id)).ToList()
            : _items;

        if (pool.Count == 0)
        {
            pool = _items;
        }

        return pool[_random.Next(pool.Count)];
    }
}
