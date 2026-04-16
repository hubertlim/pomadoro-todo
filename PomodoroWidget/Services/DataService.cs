using System.IO;
using System.Text.Json;
using PomodoroWidget.Models;

namespace PomodoroWidget.Services;

public static class DataService
{
    private const int CurrentDataVersion = 2;
    private const int MaxAutoBackups = 20;

    private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PomodoroWidget");
    private static readonly string FilePath = Path.Combine(Dir, "data.json");
    private static readonly string BackupDir = Path.Combine(Dir, "backups");
    private static readonly JsonSerializerOptions Opts = new() { WriteIndented = true };

    public static AppData Load()
    {
        try
        {
            if (File.Exists(FilePath))
                return Normalize(JsonSerializer.Deserialize<AppData>(File.ReadAllText(FilePath), Opts) ?? new());
        }
        catch
        {
            var backup = TryLoadLatestBackup();
            if (backup != null) return backup;
        }

        return new AppData();
    }

    public static void Save(AppData data)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            Directory.CreateDirectory(BackupDir);

            data.DataVersion = CurrentDataVersion;
            data.LastSavedAt = DateTimeOffset.Now.ToString("O");

            BackupCurrentFile();

            var tempPath = Path.Combine(Dir, $"data.{Guid.NewGuid():N}.tmp");
            File.WriteAllText(tempPath, JsonSerializer.Serialize(data, Opts));
            File.Copy(tempPath, FilePath, overwrite: true);
            File.Delete(tempPath);

            PruneBackups();
        }
        catch
        {
            // Persistence should never crash the widget. A future diagnostics view can surface this.
        }
    }

    public static string CreateManualBackup(AppData data)
    {
        Directory.CreateDirectory(BackupDir);
        data.DataVersion = CurrentDataVersion;
        data.LastSavedAt = DateTimeOffset.Now.ToString("O");

        var path = Path.Combine(BackupDir, $"manual-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data, Opts));
        return path;
    }

    private static AppData Normalize(AppData data)
    {
        if (data.DataVersion <= 0)
            data.DataVersion = 1;

        if (string.IsNullOrWhiteSpace(data.LastSavedAt))
            data.LastSavedAt = DateTimeOffset.Now.ToString("O");

        return data;
    }

    private static void BackupCurrentFile()
    {
        if (!File.Exists(FilePath)) return;

        var backupPath = Path.Combine(BackupDir, $"auto-{DateTime.Now:yyyyMMdd-HHmmss-fff}.json");
        File.Copy(FilePath, backupPath, overwrite: false);
    }

    private static AppData? TryLoadLatestBackup()
    {
        try
        {
            if (!Directory.Exists(BackupDir)) return null;

            var latest = Directory
                .EnumerateFiles(BackupDir, "*.json")
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .FirstOrDefault();

            if (latest == null) return null;

            return Normalize(JsonSerializer.Deserialize<AppData>(File.ReadAllText(latest.FullName), Opts) ?? new());
        }
        catch
        {
            return null;
        }
    }

    private static void PruneBackups()
    {
        if (!Directory.Exists(BackupDir)) return;

        var autoBackups = Directory
            .EnumerateFiles(BackupDir, "auto-*.json")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .Skip(MaxAutoBackups);

        foreach (var file in autoBackups)
        {
            try { file.Delete(); }
            catch { /* best-effort cleanup */ }
        }
    }
}
