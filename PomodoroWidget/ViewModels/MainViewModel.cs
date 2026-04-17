using System.Windows.Input;
using System.Windows.Threading;
using System.Globalization;
using System.Diagnostics;
using System.IO;
using System.ComponentModel;
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
    private AppData _lastLoadedData = new();

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

    public string WorkflowNudgeText
    {
        get
        {
            if (Todo.HasReviewTasks) return "Review yesterday before planning today";
            if (IsShutdownVisible) return Todo.OpenTasksCount > 0
                ? "Close the loop or move open work to tomorrow"
                : "Day complete. Save the win";
            if (IsDashboardVisible) return Analytics.WeeklyInsightText;
            if (IsFocusVisible) return BuildFocusNudge();
            if (Todo.IsEmpty) return "Add one clear task for today";
            if (Todo.NeedsTopTask) return "Pick the task that makes today a win";
            if (Todo.OpenTasksCount == 0) return "All tasks done. Finish the day";
            if (Analytics.TodayPomodoros >= Analytics.DailyPomodoroGoal) return "Daily focus goal complete";
            return "Start the next focus block";
        }
    }

    private string _backupStatusText = "Backups are saved locally.";
    public string BackupStatusText
    {
        get => _backupStatusText;
        private set => SetProperty(ref _backupStatusText, value);
    }

    private DataStatus _dataStatus = new();
    public DataStatus DataStatus
    {
        get => _dataStatus;
        private set => SetProperty(ref _dataStatus, value);
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
    public ICommand ExportDataCommand { get; }
    public ICommand ImportDataCommand { get; }
    public ICommand RestoreLatestBackupCommand { get; }
    public ICommand ExportTasksCsvCommand { get; }
    public ICommand ExportDailyReportCommand { get; }
    public ICommand SaveNowCommand { get; }
    public ICommand OpenDataFolderCommand { get; }
    public ICommand MoveOpenTasksToTomorrowCommand { get; }

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
        ExportDataCommand = new RelayCommand(_ => ExportData());
        ImportDataCommand = new RelayCommand(_ => ImportData());
        RestoreLatestBackupCommand = new RelayCommand(_ => RestoreLatestBackup());
        ExportTasksCsvCommand = new RelayCommand(_ => ExportTasksCsv());
        ExportDailyReportCommand = new RelayCommand(_ => ExportDailyReport());
        SaveNowCommand = new RelayCommand(_ => SaveNow());
        OpenDataFolderCommand = new RelayCommand(_ => OpenDataFolder());
        MoveOpenTasksToTomorrowCommand = new RelayCommand(_ => MoveOpenTasksToTomorrow());

        _autoSave = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _autoSave.Tick += (_, _) => { _autoSave.Stop(); Save(_lastLeft, _lastTop); };

        Timer.WorkPhaseCompleted += () =>
        {
            Todo.IncrementLinkedPomodoro();
            Analytics.RecordPomodoro(Timer.WorkMinutes);
            RefreshWorkflowText();
            ScheduleSave();
        };
        Timer.PhaseChanged += phase =>
        {
            if (Todo.LinkedTask != null)
                Todo.LinkedTask.IsActive = phase == Phase.Work;

            var msg = phase == Phase.Work ? "Break over! Focus time." : "Work complete! Take a break.";
            NotificationService.NotifyPhaseChange(msg);
            RefreshWorkflowText();
        };
        Timer.TimerFinished += () =>
        {
            if (Todo.LinkedTask != null)
                Todo.LinkedTask.IsActive = false;
            NotificationService.NotifyPhaseChange("Task time is up!");
            RefreshWorkflowText();
            ScheduleSave();
        };
        Timer.PropertyChanged += OnTimerPropertyChanged;

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
        ApplyData(DataService.Load());
    }

    private void ApplyData(AppData data)
    {
        _lastLoadedData = data;
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
        RefreshDataStatus();
    }

    public void Save(double left, double top)
    {
        _lastLeft = left;
        _lastTop = top;
        var snapshot = CreateSnapshot(left, top);
        DataService.Save(snapshot);
        _lastLoadedData = snapshot;
        RefreshDataStatus();
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
        OnPropertyChanged(nameof(WorkflowNudgeText));
    }

    private void RefreshSurfaceVisibility()
    {
        OnPropertyChanged(nameof(IsReviewVisible));
        OnPropertyChanged(nameof(IsPlanVisible));
        OnPropertyChanged(nameof(WorkflowNudgeText));
    }

    private string BuildFocusNudge()
    {
        if (Todo.LinkedTask == null) return "Choose a focus task";
        if (Timer.IsRunning && Timer.CurrentPhase == Phase.Work) return $"Working: {ShortTask(Todo.LinkedTask.Text)}";
        if (Timer.IsRunning && Timer.CurrentPhase == Phase.Rest) return "Break now. Return when the timer calls";
        return $"Ready: {ShortTask(Todo.LinkedTask.Text)}";
    }

    private static string ShortTask(string text)
        => text.Length <= 34 ? text : $"{text[..31]}...";

    private void OnTimerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TimerViewModel.IsRunning) or nameof(TimerViewModel.CurrentPhase))
            RefreshWorkflowText();
    }

    private void CreateManualBackup()
    {
        try
        {
            var data = CreateSnapshot(_lastLeft, _lastTop);
            var path = DataService.CreateManualBackup(data);
            BackupStatusText = $"Backup saved: {Path.GetFileName(path)}";
            RefreshDataStatus();
        }
        catch
        {
            BackupStatusText = "Backup could not be saved.";
        }
    }

    private void ExportData()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Pomodoro data",
                Filter = "Pomodoro data (*.json)|*.json|All files (*.*)|*.*",
                FileName = $"pomodoro-data-{DateTime.Now:yyyyMMdd-HHmmss}.json"
            };

            if (dialog.ShowDialog() != true) return;

            DataService.Export(CreateSnapshot(_lastLeft, _lastTop), dialog.FileName);
            BackupStatusText = $"Exported: {Path.GetFileName(dialog.FileName)}";
            RefreshDataStatus();
        }
        catch
        {
            BackupStatusText = "Export could not be saved.";
        }
    }

    private void ExportTasksCsv()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export tasks CSV",
                Filter = "CSV file (*.csv)|*.csv|All files (*.*)|*.*",
                FileName = $"pomodoro-tasks-{DateTime.Now:yyyyMMdd-HHmmss}.csv"
            };

            if (dialog.ShowDialog() != true) return;

            DataService.ExportTasksCsv(CreateSnapshot(_lastLeft, _lastTop), dialog.FileName);
            BackupStatusText = $"CSV exported: {Path.GetFileName(dialog.FileName)}";
            RefreshDataStatus();
        }
        catch
        {
            BackupStatusText = "CSV export could not be saved.";
        }
    }

    private void ExportDailyReport()
    {
        try
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export daily report",
                Filter = "Markdown file (*.md)|*.md|Text file (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = $"pomodoro-report-{DateTime.Now:yyyyMMdd}.md"
            };

            if (dialog.ShowDialog() != true) return;

            DataService.ExportDailyReport(CreateSnapshot(_lastLeft, _lastTop), dialog.FileName);
            BackupStatusText = $"Report exported: {Path.GetFileName(dialog.FileName)}";
            RefreshDataStatus();
        }
        catch
        {
            BackupStatusText = "Report export could not be saved.";
        }
    }

    private void ImportData()
    {
        try
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import Pomodoro data",
                Filter = "Pomodoro data (*.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() != true) return;

            DataService.CreateManualBackup(CreateSnapshot(_lastLeft, _lastTop));
            var imported = DataService.Import(dialog.FileName);
            ApplyData(imported);
            DataService.Save(CreateSnapshot(_lastLeft, _lastTop));
            BackupStatusText = $"Imported: {Path.GetFileName(dialog.FileName)}";
            RefreshDataStatus();
        }
        catch
        {
            BackupStatusText = "Import failed. Current data was left unchanged.";
        }
    }

    private void RestoreLatestBackup()
    {
        try
        {
            var backupPath = DataService.GetLatestBackupPath();
            if (string.IsNullOrWhiteSpace(backupPath))
            {
                BackupStatusText = "No backup is available to restore.";
                return;
            }

            DataService.CreateManualBackup(CreateSnapshot(_lastLeft, _lastTop));
            var restored = DataService.RestoreBackup(backupPath);
            ApplyData(restored);
            DataService.Save(CreateSnapshot(_lastLeft, _lastTop));
            BackupStatusText = $"Restored: {Path.GetFileName(backupPath)}";
            RefreshDataStatus();
        }
        catch
        {
            BackupStatusText = "Restore failed. Current data was left unchanged.";
        }
    }

    private void SaveNow()
    {
        Save(_lastLeft, _lastTop);
        BackupStatusText = "Saved now.";
    }

    private void OpenDataFolder()
    {
        try
        {
            Directory.CreateDirectory(DataService.DataDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = DataService.DataDirectory,
                UseShellExecute = true
            });
            BackupStatusText = "Opened data folder.";
        }
        catch
        {
            BackupStatusText = "Data folder could not be opened.";
        }
    }

    private void MoveOpenTasksToTomorrow()
    {
        var moved = Todo.MoveOpenTasksToTomorrow();
        if (moved == 0)
        {
            BackupStatusText = "No open tasks to move.";
            return;
        }

        Save(_lastLeft, _lastTop);
        BackupStatusText = moved == 1
            ? "Moved 1 open task to tomorrow."
            : $"Moved {moved} open tasks to tomorrow.";
        RefreshWorkflowText();
    }

    private void RefreshDataStatus()
        => DataStatus = DataService.GetStatus(_lastLoadedData);

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
