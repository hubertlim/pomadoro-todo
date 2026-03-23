using System.IO;
using System.Text.Json;
using PomodoroWidget.Models;

namespace PomodoroWidget.Services;

public static class DataService
{
    private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PomodoroWidget");
    private static readonly string FilePath = Path.Combine(Dir, "data.json");
    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    public static AppData Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return JsonSerializer.Deserialize<AppData>(File.ReadAllText(FilePath), Opts) ?? new();
        }
        catch { /* corrupted file, start fresh */ }
        return new AppData();
    }

    public static void Save(AppData data)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(data, Opts));
        }
        catch { /* silently fail on save errors */ }
    }
}
