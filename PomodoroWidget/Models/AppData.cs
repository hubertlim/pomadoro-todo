namespace PomodoroWidget.Models;

public class AppData
{
    public List<TodoItem> Todos { get; set; } = [];
    public int WorkMinutes { get; set; } = 25;
    public int RestMinutes { get; set; } = 5;
    public int TotalMinutes { get; set; } = 120;
    public double WindowLeft { get; set; } = 100;
    public double WindowTop { get; set; } = 100;
    public bool IsExpanded { get; set; } = true;
}
