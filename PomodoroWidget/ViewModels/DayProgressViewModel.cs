using PomodoroWidget.Models;

namespace PomodoroWidget.ViewModels;

public class DayProgressViewModel : ViewModelBase
{
    private readonly DailyProgress _model;

    public DayProgressViewModel(DailyProgress model, string label, int dailyGoal, bool isToday)
    {
        _model = model;
        DayLabel = label;
        DailyGoal = dailyGoal;
        IsToday = isToday;
    }

    public string DayLabel { get; }
    public int DailyGoal { get; }
    public bool IsToday { get; }
    public int PomodorosCompleted => _model.PomodorosCompleted;
    public int TasksCompleted => _model.TasksCompleted;
    public int FocusMinutes => _model.FocusMinutes;
    public int TasksCarriedOver => _model.TasksCarriedOver;
    public double GoalPercent => DailyGoal > 0
        ? Math.Min(100, (double)PomodorosCompleted / DailyGoal * 100)
        : 0;
    public string SummaryText => $"{PomodorosCompleted} focus / {TasksCompleted} done";
}
