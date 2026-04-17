using System.IO;
using System.Globalization;
using System.Text.Json;
using System.Text;
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

            PrepareForWrite(data);

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
        PrepareForWrite(data);

        var path = Path.Combine(BackupDir, $"manual-{DateTime.Now:yyyyMMdd-HHmmss}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(data, Opts));
        return path;
    }

    public static void Export(AppData data, string path)
    {
        PrepareForWrite(data);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Dir);
        File.WriteAllText(path, JsonSerializer.Serialize(data, Opts));
    }

    public static void ExportTasksCsv(AppData data, string path)
    {
        PrepareForWrite(data);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Dir);

        var csv = new StringBuilder();
        csv.AppendLine("Id,Text,Priority,Completed,PlannedFor,CompletedDate,EstimatedPomodoros,PomodoroCount,RolloverCount");

        foreach (var item in data.Todos)
        {
            csv.AppendLine(string.Join(",",
                Csv(item.Id),
                Csv(item.Text),
                Csv(item.Priority.ToString()),
                Csv(item.IsCompleted ? "Yes" : "No"),
                Csv(item.PlannedForDate),
                Csv(item.CompletedDate ?? string.Empty),
                item.EstimatedPomodoros.ToString(CultureInfo.InvariantCulture),
                item.PomodoroCount.ToString(CultureInfo.InvariantCulture),
                item.RolloverCount.ToString(CultureInfo.InvariantCulture)));
        }

        File.WriteAllText(path, csv.ToString());
    }

    public static void ExportDailyReport(AppData data, string path)
    {
        PrepareForWrite(data);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Dir);

        var today = DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var todayProgress = data.DailyProgress.FirstOrDefault(day => day.Date == today) ?? new DailyProgress { Date = today };
        var openTasks = data.Todos.Where(task => !task.IsCompleted && task.PlannedForDate == today).ToList();
        var completedTasks = data.Todos.Where(task => task.IsCompleted && task.CompletedDate == today).ToList();

        var report = new StringBuilder();
        report.AppendLine($"# Pomodoro Report - {today}");
        report.AppendLine();
        report.AppendLine($"- Focus blocks: {todayProgress.PomodorosCompleted}/{data.DailyPomodoroGoal}");
        report.AppendLine($"- Focus minutes: {todayProgress.FocusMinutes}");
        report.AppendLine($"- Tasks planned: {todayProgress.TasksCreated}");
        report.AppendLine($"- Tasks completed: {todayProgress.TasksCompleted}");
        report.AppendLine($"- Tasks carried over: {todayProgress.TasksCarriedOver}");
        report.AppendLine($"- Current streak: {data.CurrentStreak}");
        report.AppendLine();
        report.AppendLine("## Completed Today");
        AppendTaskList(report, completedTasks);
        report.AppendLine();
        report.AppendLine("## Open For Today");
        AppendTaskList(report, openTasks);
        report.AppendLine();
        report.AppendLine("## Data");
        report.AppendLine($"- Data version: {data.DataVersion}");
        report.AppendLine($"- Exported: {data.LastSavedAt}");

        File.WriteAllText(path, report.ToString());
    }

    public static AppData Import(string path)
        => Normalize(JsonSerializer.Deserialize<AppData>(File.ReadAllText(path), Opts) ?? new());

    public static string? GetLatestBackupPath()
        => GetLatestBackupFile()?.FullName;

    public static AppData RestoreBackup(string path)
        => Import(path);

    public static DataStatus GetStatus(AppData data)
    {
        var dataFile = new FileInfo(FilePath);
        var backups = Directory.Exists(BackupDir)
            ? Directory
                .EnumerateFiles(BackupDir, "*.json")
                .Select(path => new FileInfo(path))
                .OrderByDescending(file => file.LastWriteTimeUtc)
                .ToList()
            : [];
        var latestBackup = backups.FirstOrDefault();
        var dataFileExists = dataFile.Exists;
        var backupDirectoryExists = Directory.Exists(BackupDir);

        return new DataStatus
        {
            DataFilePath = FilePath,
            BackupDirectoryPath = BackupDir,
            DataFileExists = dataFileExists,
            BackupDirectoryExists = backupDirectoryExists,
            HealthText = BuildHealthText(dataFileExists, backupDirectoryExists, backups.Count),
            DataVersion = data.DataVersion,
            LastSavedText = FormatTimestamp(data.LastSavedAt),
            LatestBackupText = latestBackup != null
                ? $"{latestBackup.Name} ({latestBackup.LastWriteTime:g})"
                : "No backups yet",
            DataFileSizeText = dataFileExists ? FormatBytes(dataFile.Length) : "0 KB",
            LatestBackupSizeText = latestBackup != null ? FormatBytes(latestBackup.Length) : "0 KB",
            BackupCount = backups.Count
        };
    }

    private static AppData Normalize(AppData data)
    {
        if (data.DataVersion <= 0)
            data.DataVersion = 1;

        if (string.IsNullOrWhiteSpace(data.LastSavedAt))
            data.LastSavedAt = DateTimeOffset.Now.ToString("O");

        data.Todos = data.Todos?.Where(item => item != null).ToList() ?? [];
        data.DailyProgress = data.DailyProgress?.Where(day => day != null).ToList() ?? [];
        data.WorkMinutes = Math.Clamp(data.WorkMinutes, 1, 120);
        data.RestMinutes = Math.Clamp(data.RestMinutes, 1, 60);
        data.TotalMinutes = Math.Clamp(data.TotalMinutes, 1, 480);
        data.DailyPomodoroGoal = Math.Clamp(data.DailyPomodoroGoal, 1, 12);
        data.CurrentStreak = Math.Max(0, data.CurrentStreak);
        data.LongestStreak = Math.Max(data.CurrentStreak, data.LongestStreak);
        data.WindowLeft = NormalizeWindowPosition(data.WindowLeft, 100);
        data.WindowTop = NormalizeWindowPosition(data.WindowTop, 100);

        var seenTaskIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in data.Todos)
        {
            NormalizeTodo(item);
            while (!seenTaskIds.Add(item.Id))
                item.Id = Guid.NewGuid().ToString("N")[..8];
        }

        foreach (var day in data.DailyProgress)
            NormalizeDailyProgress(day);

        data.DailyProgress = data.DailyProgress
            .GroupBy(day => day.Date, StringComparer.Ordinal)
            .Select(group => new DailyProgress
            {
                Date = group.Key,
                CheckedIn = group.Any(day => day.CheckedIn),
                TasksCreated = group.Sum(day => day.TasksCreated),
                TasksCompleted = group.Sum(day => day.TasksCompleted),
                PomodorosCompleted = group.Sum(day => day.PomodorosCompleted),
                FocusMinutes = group.Sum(day => day.FocusMinutes),
                TasksCarriedOver = group.Sum(day => day.TasksCarriedOver)
            })
            .OrderBy(day => day.Date, StringComparer.Ordinal)
            .ToList();

        return data;
    }

    private static void NormalizeTodo(TodoItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Id))
            item.Id = Guid.NewGuid().ToString("N")[..8];

        item.Text = (item.Text ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(item.Text))
            item.Text = "Untitled task";

        if (!Enum.IsDefined(item.Priority))
            item.Priority = Priority.Medium;

        item.PomodoroCount = Math.Max(0, item.PomodoroCount);
        item.EstimatedPomodoros = Math.Clamp(item.EstimatedPomodoros, 1, 8);
        item.CreatedAt = item.CreatedAt > 0 ? item.CreatedAt : DateTimeOffset.Now.ToUnixTimeMilliseconds();
        item.PlannedForDate = NormalizeDateKey(item.PlannedForDate, DateKeyFromUnix(item.CreatedAt));
        item.CompletedDate = string.IsNullOrWhiteSpace(item.CompletedDate)
            ? null
            : NormalizeDateKey(item.CompletedDate, item.PlannedForDate);
        item.RolloverCount = Math.Max(0, item.RolloverCount);
    }

    private static void NormalizeDailyProgress(DailyProgress day)
    {
        day.Date = NormalizeDateKey(day.Date, DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        day.TasksCreated = Math.Max(0, day.TasksCreated);
        day.TasksCompleted = Math.Max(0, day.TasksCompleted);
        day.PomodorosCompleted = Math.Max(0, day.PomodorosCompleted);
        day.FocusMinutes = Math.Max(0, day.FocusMinutes);
        day.TasksCarriedOver = Math.Max(0, day.TasksCarriedOver);
    }

    private static string NormalizeDateKey(string? value, string fallback)
        => DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed)
            ? parsed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : fallback;

    private static string DateKeyFromUnix(long unixMilliseconds)
        => DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds)
            .LocalDateTime
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static double NormalizeWindowPosition(double value, double fallback)
        => double.IsFinite(value) && value >= 0 ? value : fallback;

    private static void PrepareForWrite(AppData data)
    {
        data.DataVersion = CurrentDataVersion;
        data.LastSavedAt = DateTimeOffset.Now.ToString("O");
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
            var latest = GetLatestBackupFile();

            if (latest == null) return null;

            return Normalize(JsonSerializer.Deserialize<AppData>(File.ReadAllText(latest.FullName), Opts) ?? new());
        }
        catch
        {
            return null;
        }
    }

    private static FileInfo? GetLatestBackupFile()
    {
        if (!Directory.Exists(BackupDir)) return null;

        return Directory
            .EnumerateFiles(BackupDir, "*.json")
            .Select(path => new FileInfo(path))
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .FirstOrDefault();
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

    private static string FormatTimestamp(string value)
    {
        if (DateTimeOffset.TryParse(value, out var timestamp))
            return timestamp.LocalDateTime.ToString("g");

        return "Unknown";
    }

    public static string DataDirectory => Dir;

    private static string BuildHealthText(bool dataFileExists, bool backupDirectoryExists, int backupCount)
    {
        if (!dataFileExists) return "No save file yet";
        if (!backupDirectoryExists || backupCount == 0) return "Saved, backup pending";
        return "Healthy";
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        var kb = bytes / 1024d;
        if (kb < 1024) return $"{kb:0.#} KB";
        return $"{kb / 1024d:0.#} MB";
    }

    private static string Csv(string value)
        => $"\"{value.Replace("\"", "\"\"")}\"";

    private static void AppendTaskList(StringBuilder report, IReadOnlyCollection<TodoItem> tasks)
    {
        if (tasks.Count == 0)
        {
            report.AppendLine("- None");
            return;
        }

        foreach (var task in tasks)
            report.AppendLine($"- {task.Text} ({task.Priority}, {task.EstimatedPomodoros} block(s), P{task.PomodoroCount})");
    }
}
