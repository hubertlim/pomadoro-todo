namespace PomodoroWidget.Models;

public class DailyProgress
{
    public string Date { get; set; } = string.Empty;
    public bool CheckedIn { get; set; }
    public int TasksCreated { get; set; }
    public int TasksCompleted { get; set; }
    public int PomodorosCompleted { get; set; }
    public int FocusMinutes { get; set; }
    public int TasksCarriedOver { get; set; }
}
