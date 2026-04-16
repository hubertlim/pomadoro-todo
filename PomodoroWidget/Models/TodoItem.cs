namespace PomodoroWidget.Models;

public enum Priority { Low, Medium, High }

public class TodoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public Priority Priority { get; set; } = Priority.Medium;
    public int PomodoroCount { get; set; }
    public int EstimatedPomodoros { get; set; } = 1;
    public long CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string PlannedForDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
    public string? CompletedDate { get; set; }
    public long? CompletedAt { get; set; }
    public int RolloverCount { get; set; }
}
