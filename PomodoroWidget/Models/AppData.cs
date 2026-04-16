namespace PomodoroWidget.Models;

public class AppData
{
    public int DataVersion { get; set; } = 2;
    public string LastSavedAt { get; set; } = DateTimeOffset.Now.ToString("O");
    public List<TodoItem> Todos { get; set; } = [];
    public List<DailyProgress> DailyProgress { get; set; } = [];
    public int WorkMinutes { get; set; } = 25;
    public int RestMinutes { get; set; } = 5;
    public int TotalMinutes { get; set; } = 120;
    public int DailyPomodoroGoal { get; set; } = 4;
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public string? LastStreakDate { get; set; }
    public double WindowLeft { get; set; } = 100;
    public double WindowTop { get; set; } = 100;
    public bool IsExpanded { get; set; } = true;
    public bool IsDashboardVisible { get; set; }
}
