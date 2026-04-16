using System.Windows.Input;
using System.Windows.Threading;
using System.Globalization;
using System.IO;
using PomodoroWidget.Models;
using PomodoroWidget.Services;

namespace PomodoroWidget.ViewModels;

public class MainViewModel : ViewModelBase
{
    public TodoViewModel Todo { get; } = new();
    public TimerViewModel Timer { get; } = new();
    public AnalyticsViewModel Analytics { get; } = new();

    // Auto-save: debounced 3-second timer
    private readonly DispatcherTimer _autoSave;
    private double _lastLeft, _lastTop;

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set { if (SetProperty(ref _isExpanded, value)) ScheduleSave(); }
    }

    private bool _isDashboardVisible;
    public bool IsDashboardVisible
    {
        get => _isDashboardVisible;
        set
        {
            if (!SetProperty(ref _isDashboardVisible, value)) return;
            if (value)
            {
                IsFocusVisible = false;
                IsShutdownVisible = false;
            }
            RefreshSurfaceVisibility();
            ScheduleSave();
        }
    }

    private bool _isFocusVisible;
    public bool IsFocusVisible
    {
        get => _isFocusVisible;
        private set
        {
            if (!SetProperty(ref _isFocusVisible, value)) return;
            if (value)
            {
                IsDashboardVisible = false;
                IsShutdownVisible = false;
            }
            RefreshSurfaceVisibility();
            ScheduleSave();
        }
    }

    private bool _isShutdownVisible;
    public bool IsShutdownVisible
    {
        get => _isShutdownVisible;
        private set
        {
            if (!SetProperty(ref _isShutdownVisible, value)) return;
            if (value)
            {
                IsDashboardVisible = false;
                IsFocusVisible = false;
            }
            RefreshSurfaceVisibility();
            ScheduleSave();
        }
    }

    public bool IsReviewVisible => Todo.HasReviewTasks && !IsDashboardVisible && !IsFocusVisible && !IsShutdownVisible;
    public bool IsPlanVisible => !Todo.HasReviewTasks && !IsDashboardVisible && !IsFocusVisible && !IsShutdownVisible;
    public string PlanCapacityText
    {
        get
        {
            if (Todo.IsEmpty) return "Choose the first task and keep the plan light.";
            if (Todo.PlannedFocusBlocks <= Analytics.DailyPomodoroGoal)
                return $"{Todo.PlannedFocusBlocks}/{Analytics.DailyPomodoroGoal} focus blocks planned. Good size for today.";
            return $"{Todo.PlannedFocusBlocks}/{Analytics.DailyPomodoroGoal} focus blocks planned. Consider moving something to tomorrow.";
        }
    }

    public string ShutdownSummaryText
        => $"{Todo.CompletedTasksCount} done, {Todo.OpenTasksCount} open, {Analytics.TodayFocusMinutes} focused minutes.";

    private string _backupStatusText = "Backups are saved locally.";
    public string BackupStatusText
    {
        get => _backupStatusText;
        private set => SetProperty(ref _backupStatusText, value);
    }

    public ICommand ToggleExpandCommand { get; }
    public ICommand ToggleDashboardCommand { get; }
    public ICommand ShowPlanCommand { get; }
    public ICommand BeginFocusCommand { get; }
    public ICommand SelectTopTaskAndFocusCommand { get; }
    public ICommand NextFocusTaskCommand { get; }
    public ICommand CompleteFocusedTaskCommand { get; }
    public ICommand BeginShutdownCommand { get; }
    public ICommand FinishShutdownCommand { get; }
    public ICommand CreateBackupCommand { get; }

    public MainViewModel()
    {
        ToggleExpandCommand = new RelayCommand(_ => IsExpanded = !IsExpanded);
        ToggleDashboardCommand = new RelayCommand(_ =>
        {
            IsDashboardVisible = !IsDashboardVisible;
            if (IsDashboardVisible)
                IsExpanded = true;
        });
        ShowPlanCommand = new RelayCommand(_ => ShowPlan());
        BeginFocusCommand = new RelayCommand(_ => BeginFocus());
        SelectTopTaskAndFocusCommand = new RelayCommand(_ =>
        {
            Todo.LinkFirstOpenTask();
            BeginFocus();
        });
        NextFocusTaskCommand = new RelayCommand(_ => { Todo.LinkNextOpenTask(); RefreshWorkflowText(); });
        CompleteFocusedTaskCommand = new RelayCommand(_ => { Todo.CompleteLinkedTask(); RefreshWorkflowText(); });
        BeginShutdownCommand = new RelayCommand(_ => BeginShutdown());
        FinishShutdownCommand = new RelayCommand(_ => ShowPlan());
        CreateBackupCommand = new RelayCommand(_ => CreateManualBackup());

        _autoSave = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _autoSave.Tick += (_, _) => { _autoSave.Stop(); Save(_lastLeft, _lastTop); };

        Timer.WorkPhaseCompleted += () =>
        {
            Todo.IncrementLinkedPomodoro();
            Analytics.RecordPomodoro(Timer.WorkMinutes);
            ScheduleSave();
        };
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
        Todo.Changed += () => { RefreshWorkflowText(); RefreshSurfaceVisibility(); ScheduleSave(); };
        Todo.TaskAdded += _ => Analytics.RecordTaskCreated();
        Todo.TaskCompletionChanged += (_, isCompleted) => Analytics.RecordTaskCompletionChanged(isCompleted);
        Analytics.Changed += () => { RefreshWorkflowText(); ScheduleSave(); };

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
        var todayKey = TodayKey();
        Analytics.LoadFrom(data.DailyProgress, data.DailyPomodoroGoal, data.CurrentStreak, data.LongestStreak, data.LastStreakDate);
        var carriedOver = Todo.LoadFrom(data.Todos, todayKey);
        Analytics.StartToday(todayKey, carriedOver);
        Timer.WorkMinutes = data.WorkMinutes;
        Timer.RestMinutes = data.RestMinutes;
        Timer.TotalMinutes = data.TotalMinutes;
        IsExpanded = data.IsExpanded;
        IsDashboardVisible = data.IsDashboardVisible;
        if (Todo.HasReviewTasks)
            ShowPlan();
    }

    public void Save(double left, double top)
    {
        _lastLeft = left;
        _lastTop = top;
        DataService.Save(CreateSnapshot(left, top));
    }

    public (double Left, double Top) GetSavedPosition()
    {
        var data = DataService.Load();
        return (data.WindowLeft, data.WindowTop);
    }

    private static string TodayKey()
        => DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private void ShowPlan()
    {
        IsDashboardVisible = false;
        IsFocusVisible = false;
        IsShutdownVisible = false;
        IsExpanded = true;
    }

    private void BeginFocus()
    {
        if (Todo.HasReviewTasks)
        {
            ShowPlan();
            return;
        }

        if (Todo.LinkedTask == null)
            Todo.LinkFirstOpenTask();

        if (!Todo.HasOpenTasks)
        {
            BeginShutdown();
            return;
        }

        IsFocusVisible = true;
        IsExpanded = true;
        RefreshWorkflowText();
    }

    private void BeginShutdown()
    {
        IsShutdownVisible = true;
        IsExpanded = true;
        RefreshWorkflowText();
    }

    private void RefreshWorkflowText()
    {
        OnPropertyChanged(nameof(PlanCapacityText));
        OnPropertyChanged(nameof(ShutdownSummaryText));
    }

    private void RefreshSurfaceVisibility()
    {
        OnPropertyChanged(nameof(IsReviewVisible));
        OnPropertyChanged(nameof(IsPlanVisible));
    }

    private void CreateManualBackup()
    {
        try
        {
            var data = CreateSnapshot(_lastLeft, _lastTop);
            var path = DataService.CreateManualBackup(data);
            BackupStatusText = $"Backup saved: {Path.GetFileName(path)}";
        }
        catch
        {
            BackupStatusText = "Backup could not be saved.";
        }
    }

    private AppData CreateSnapshot(double left, double top)
        => new()
        {
            Todos = Todo.ToModels(),
            DailyProgress = Analytics.ToModels(),
            WorkMinutes = Timer.WorkMinutes,
            RestMinutes = Timer.RestMinutes,
            TotalMinutes = Timer.TotalMinutes,
            DailyPomodoroGoal = Analytics.DailyPomodoroGoal,
            CurrentStreak = Analytics.CurrentStreak,
            LongestStreak = Analytics.LongestStreak,
            LastStreakDate = Analytics.LastStreakDate,
            WindowLeft = left,
            WindowTop = top,
            IsExpanded = IsExpanded,
            IsDashboardVisible = IsDashboardVisible,
        };
}
