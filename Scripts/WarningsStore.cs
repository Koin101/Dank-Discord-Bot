using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Discord_Bot.Scripts;

public record Warning(string Reason, DateTime IssuedAt);

public static class WarningsStore
{
    private static readonly string FilePath = Path.Combine("Data", "warnings.json");
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private static Dictionary<ulong, List<Warning>> Load()
    {
        if (!File.Exists(FilePath))
            return new Dictionary<ulong, List<Warning>>();

        var json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<Dictionary<ulong, List<Warning>>>(json, JsonOptions)
               ?? new Dictionary<ulong, List<Warning>>();
    }

    private static void Save(Dictionary<ulong, List<Warning>> data)
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(data, JsonOptions));
    }

    public static void AddWarning(ulong userId, string reason)
    {
        var data = Load();
        if (!data.ContainsKey(userId))
            data[userId] = new List<Warning>();

        data[userId].Add(new Warning(reason, DateTime.UtcNow));
        Save(data);
    }

    public static List<Warning> GetWarnings(ulong userId)
    {
        var data = Load();
        return data.TryGetValue(userId, out var warnings) ? warnings : new List<Warning>();
    }
}
