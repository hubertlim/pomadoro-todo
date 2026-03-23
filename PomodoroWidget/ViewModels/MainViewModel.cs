using System.Windows.Input;
using System.Windows.Threading;
using PomodoroWidget.Models;
using PomodoroWidget.Services;

namespace PomodoroWidget.ViewModels;

public class MainViewModel : ViewModelBase
{
    public TodoViewModel Todo { get; } = new();
    public TimerViewModel Timer { get; } = new();

    // Auto-save: debounced 3-second timer
    private readonly DispatcherTimer _autoSave;
    private double _lastLeft, _lastTop;

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { if (SetProperty(ref _isExpanded, value)) ScheduleSave(); }
    }

    public ICommand ToggleExpandCommand { get; }

    public MainViewModel()
    {
        ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);

        _autoSave = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _autoSave.Tick += (_, _) => { _autoSave.Stop(); Save(_lastLeft, _lastTop); };

        Timer.WorkPhaseCompleted += () => { Todo.IncrementLinkedPomodoro(); ScheduleSave(); };
        Timer.PhaseChanged += phase =>
        {
            if (Todo.LinkedTask != null)
                Todo.LinkedTask.IsActive = phase == Phase.Work;

            var msg = phase == Phase.Work ? "Break over! Focus time." : "Work complete! Take a break.";
            NotificationService.NotifyPhaseChange(msg);
        };
        Timer.TimerFinished += () =>
        {
            if (Todo.LinkedTask != null)
                Todo.LinkedTask.IsActive = false;
            NotificationService.NotifyPhaseChange("Task time is up!");
            ScheduleSave();
        };

        // Listen for todo changes
        Todo.Todos.CollectionChanged += (_, _) => ScheduleSave();

        Load();
    }

    public void ScheduleSave()
    {
        _autoSave.Stop();
        _autoSave.Start(); // resets the 3s debounce
    }

    public void UpdatePosition(double left, double top)
    {
        _lastLeft = left;
        _lastTop = top;
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
        _lastLeft = left;
        _lastTop = top;
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
