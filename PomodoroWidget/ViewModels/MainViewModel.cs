using System.Windows.Input;
using PomodoroWidget.Models;
using PomodoroWidget.Services;

namespace PomodoroWidget.ViewModels;

public class MainViewModel : ViewModelBase
{
    public TodoViewModel Todo { get; } = new();
    public TimerViewModel Timer { get; } = new();

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }

    public ICommand ToggleExpandCommand { get; }

    public MainViewModel()
    {
        ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);

        Timer.WorkPhaseCompleted += () => Todo.IncrementLinkedPomodoro();
        Timer.PhaseChanged += phase =>
        {
            if (Todo.LinkedTask != null)
                Todo.LinkedTask.IsActive = phase == Phase.Work;
        };
        Timer.TimerFinished += () =>
        {
            if (Todo.LinkedTask != null)
                Todo.LinkedTask.IsActive = false;
        };

        Load();
    }

    public void Load()
    {
        var data = DataService.Load();
        Todo.LoadFrom(data.Todos);
        Timer.WorkMinutes = data.WorkMinutes;
        Timer.RestMinutes = data.RestMinutes;
        Timer.TotalMinutes = data.TotalMinutes;
        IsExpanded = data.IsExpanded;
    }

    public void Save(double left, double top)
    {
        DataService.Save(new AppData
        {
            Todos = Todo.ToModels(),
            WorkMinutes = Timer.WorkMinutes,
            RestMinutes = Timer.RestMinutes,
            TotalMinutes = Timer.TotalMinutes,
            WindowLeft = left,
            WindowTop = top,
            IsExpanded = IsExpanded,
        });
    }

    public (double Left, double Top) GetSavedPosition()
    {
        var data = DataService.Load();
        return (data.WindowLeft, data.WindowTop);
    }
}
