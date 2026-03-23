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
        set { _model.IsCompleted = value; OnPropertyChanged(); }
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
        set { _model.PomodoroCount = value; OnPropertyChanged(); }
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
