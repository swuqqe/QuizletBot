using System.Text.Json;
using QuizletBot.Models;

namespace QuizletBot.Services;

// Simple JSON-file storage for per-user data. Good enough for a single-instance
// bot - if this ever needs to scale, swap this out for a real DB without
// touching the callers.
public class UserDataStore
{
    private readonly string _filePath;
    private readonly object _lock = new();
    private readonly Dictionary<long, UserData> _data;

    public UserDataStore(string filePath)
    {
        _filePath = filePath;
        _data = Load();
    }

    public UserData Get(long chatId)
    {
        lock (_lock)
        {
            if (_data.TryGetValue(chatId, out var existing))
            {
                return existing;
            }

            var created = new UserData { ChatId = chatId };
            _data[chatId] = created;
            Persist();
            return created;
        }
    }

    // The UserData object is already mutated by reference - this just writes it to disk.
    public void Save(UserData data)
    {
        lock (_lock)
        {
            _data[data.ChatId] = data;
            Persist();
        }
    }

    private Dictionary<long, UserData> Load()
    {
        if (!File.Exists(_filePath))
        {
            return new Dictionary<long, UserData>();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var list = JsonSerializer.Deserialize<List<UserData>>(json) ?? new List<UserData>();
            return list.ToDictionary(x => x.ChatId);
        }
        catch
        {
            // Corrupted file - better to start clean than crash on startup.
            return new Dictionary<long, UserData>();
        }
    }

    private void Persist()
    {
        var json = JsonSerializer.Serialize(_data.Values.ToList(), new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
