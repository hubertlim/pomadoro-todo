using PomodoroWidget.Models;

namespace PomodoroWidget.ViewModels;

public class TodoItemViewModel : ViewModelBase
{
    private readonly TodoItem _model;

    public TodoItemViewModel(TodoItem model) => _model = model;

    public string Id => _model.Id;
    public TodoItem Model => _model;

    public string Text
    {
        get => _model.Text;
        set { _model.Text = value; OnPropertyChanged(); }
    }

    public bool IsCompleted
    {
        get => _model.IsCompleted;
        set { _model.IsCompleted = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
    }

    public Priority Priority
    {
        get => _model.Priority;
        set { _model.Priority = value; OnPropertyChanged(); OnPropertyChanged(nameof(PriorityIndex)); }
    }

    // For ComboBox binding (0=Low, 1=Medium, 2=High)
    public int PriorityIndex
    {
        get => (int)_model.Priority;
        set { Priority = (Priority)value; }
    }

    public int PomodoroCount
    {
        get => _model.PomodoroCount;
        set
        {
            _model.PomodoroCount = Math.Max(0, value);
            OnPropertyChanged();
            OnPropertyChanged(nameof(FocusProgressPercent));
            OnPropertyChanged(nameof(FocusProgressText));
        }
    }

    public int EstimatedPomodoros
    {
        get => Math.Max(1, _model.EstimatedPomodoros);
        set
        {
            _model.EstimatedPomodoros = Math.Clamp(value, 1, 8);
            OnPropertyChanged();
            OnPropertyChanged(nameof(EstimateText));
            OnPropertyChanged(nameof(FocusProgressPercent));
            OnPropertyChanged(nameof(FocusProgressText));
        }
    }

    public string EstimateText => EstimatedPomodoros == 1 ? "1 block" : $"{EstimatedPomodoros} blocks";
    public double FocusProgressPercent => Math.Min(100, (double)PomodoroCount / EstimatedPomodoros * 100);
    public string FocusProgressText
    {
        get
        {
            if (PomodoroCount <= 0) return $"0/{EstimatedPomodoros} focus blocks";
            if (PomodoroCount >= EstimatedPomodoros) return $"{PomodoroCount}/{EstimatedPomodoros} blocks done";
            return $"{PomodoroCount}/{EstimatedPomodoros} blocks complete";
        }
    }

    public string PlannedForDate
    {
        get => _model.PlannedForDate;
        set { _model.PlannedForDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
    }

    public string? CompletedDate
    {
        get => _model.CompletedDate;
        set { _model.CompletedDate = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
    }

    public long? CompletedAt
    {
        get => _model.CompletedAt;
        set { _model.CompletedAt = value; OnPropertyChanged(); }
    }

    public int RolloverCount
    {
        get => _model.RolloverCount;
        set { _model.RolloverCount = value; OnPropertyChanged(); OnPropertyChanged(nameof(StatusText)); }
    }

    public string StatusText
    {
        get
        {
            if (IsCompleted) return "Done today";
            if (RolloverCount == 1) return "Rolled over once";
            if (RolloverCount > 1) return $"Rolled over {RolloverCount} times";
            return "Today";
        }
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set => SetProperty(ref _isEditing, value);
    }
}
