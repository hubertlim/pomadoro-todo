using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
using PomodoroWidget.Models;

namespace PomodoroWidget.ViewModels;

public class TodoViewModel : ViewModelBase
{
    public ObservableCollection<TodoItemViewModel> Todos { get; } = [];
    public ObservableCollection<TodoItemViewModel> ReviewTasks { get; } = [];
    private readonly List<TodoItem> _archive = [];
    private string _todayKey = TodayKey();

    public bool HasReviewTasks => ReviewTasks.Count > 0;
    public int ReviewTasksCount => ReviewTasks.Count;
    public string ReviewSummaryText => ReviewTasksCount == 1
        ? "1 unfinished task is waiting from a previous day."
        : $"{ReviewTasksCount} unfinished tasks are waiting from previous days.";
    public bool IsEmpty => Todos.Count == 0;
    public bool HasTasks => Todos.Count > 0;
    public bool HasOpenTasks => Todos.Any(t => !t.IsCompleted);
    public bool NeedsTopTask => HasOpenTasks && LinkedTask == null;
    public int OpenTasksCount => Todos.Count(t => !t.IsCompleted);
    public int CompletedTasksCount => Todos.Count(t => t.IsCompleted);
    public int PlannedFocusBlocks => Todos.Where(t => !t.IsCompleted).Sum(t => t.EstimatedPomodoros);
    public int TomorrowTasksCount => _archive.Count(IsTomorrowOpenTask);
    public int TomorrowFocusBlocks => _archive.Where(IsTomorrowOpenTask).Sum(t => t.EstimatedPomodoros);
    public string PlanSummaryText
    {
        get
        {
            if (IsEmpty) return "Start with one task for today.";
            var taskText = OpenTasksCount == 1 ? "1 open task" : $"{OpenTasksCount} open tasks";
            var blockText = PlannedFocusBlocks == 1 ? "1 focus block" : $"{PlannedFocusBlocks} focus blocks";
            return $"{taskText} planned, about {blockText}.";
        }
    }
    public string TomorrowPlanText
    {
        get
        {
            if (TomorrowTasksCount == 0) return "No tasks moved to tomorrow yet.";
            var taskText = TomorrowTasksCount == 1 ? "1 task" : $"{TomorrowTasksCount} tasks";
            var blockText = TomorrowFocusBlocks == 1 ? "1 focus block" : $"{TomorrowFocusBlocks} focus blocks";
            return $"{taskText} ready for tomorrow, about {blockText}.";
        }
    }
    public string TopTaskPromptText => OpenTasksCount == 1
        ? "Choose the first focus task and start with a clean win."
        : "Choose the task that makes today a win, then enter Focus mode.";

    private string _newTaskText = string.Empty;
    public string NewTaskText
    {
        get => _newTaskText;
        set => SetProperty(ref _newTaskText, value);
    }

    private int _newTaskPriority = 1; // Medium
    public int NewTaskPriority
    {
        get => _newTaskPriority;
        set => SetProperty(ref _newTaskPriority, value);
    }

    private TodoItemViewModel? _linkedTask;
    public TodoItemViewModel? LinkedTask
    {
        get => _linkedTask;
        set
        {
            if (_linkedTask != null) _linkedTask.IsActive = false;
            if (SetProperty(ref _linkedTask, value))
            {
                OnPropertyChanged(nameof(HasLinkedTask));
                OnPropertyChanged(nameof(LinkedTaskText));
                OnPropertyChanged(nameof(NeedsTopTask));
            }
            if (_linkedTask != null) _linkedTask.IsActive = true;
        }
    }

    public bool HasLinkedTask => LinkedTask != null;
    public string LinkedTaskText => LinkedTask?.Text ?? "Pick a task to focus.";

    public ICommand AddCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand ToggleCompleteCommand { get; }
    public ICommand ClearCompletedCommand { get; }
    public ICommand LinkToTimerCommand { get; }
    public ICommand CyclePriorityCommand { get; }
    public ICommand IncreaseEstimateCommand { get; }
    public ICommand DecreaseEstimateCommand { get; }
    public ICommand KeepReviewTaskCommand { get; }
    public ICommand DeferReviewTaskCommand { get; }
    public ICommand DropReviewTaskCommand { get; }
    public ICommand KeepAllReviewTasksCommand { get; }
    public ICommand DeferAllReviewTasksCommand { get; }
    public ICommand SelectFirstOpenTaskCommand { get; }

    public event Action? Changed;
    public event Action<TodoItemViewModel>? TaskAdded;
    public event Action<TodoItemViewModel, bool>? TaskCompletionChanged;

    public TodoViewModel()
    {
        Todos.CollectionChanged += (_, _) =>
        {
            NotifyPlanProperties();
        };
        ReviewTasks.CollectionChanged += (_, _) =>
        {
            NotifyReviewProperties();
        };

        AddCommand = new RelayCommand(_ => AddTask());
        DeleteCommand = new RelayCommand(p => DeleteTask(p as TodoItemViewModel));
        ToggleCompleteCommand = new RelayCommand(p => ToggleComplete(p as TodoItemViewModel));
        ClearCompletedCommand = new RelayCommand(_ => ClearCompleted());
        LinkToTimerCommand = new RelayCommand(p => LinkToTimer(p as TodoItemViewModel));
        CyclePriorityCommand = new RelayCommand(p => CyclePriority(p as TodoItemViewModel));
        IncreaseEstimateCommand = new RelayCommand(p => ChangeEstimate(p as TodoItemViewModel, 1));
        DecreaseEstimateCommand = new RelayCommand(p => ChangeEstimate(p as TodoItemViewModel, -1));
        KeepReviewTaskCommand = new RelayCommand(p => KeepReviewTask(p as TodoItemViewModel));
        DeferReviewTaskCommand = new RelayCommand(p => DeferReviewTask(p as TodoItemViewModel));
        DropReviewTaskCommand = new RelayCommand(p => DropReviewTask(p as TodoItemViewModel));
        KeepAllReviewTasksCommand = new RelayCommand(_ => KeepAllReviewTasks());
        DeferAllReviewTasksCommand = new RelayCommand(_ => DeferAllReviewTasks());
        SelectFirstOpenTaskCommand = new RelayCommand(_ => { LinkFirstOpenTask(); NotifyPlanProperties(); NotifyChanged(); });
    }

    private void AddTask()
    {
        var text = NewTaskText.Trim();
        if (string.IsNullOrEmpty(text)) return;
        var item = new TodoItem
        {
            Text = text,
            Priority = (Priority)NewTaskPriority,
            PlannedForDate = _todayKey
        };
        var vm = new TodoItemViewModel(item);
        WatchItem(vm);
        Todos.Insert(0, vm);
        NewTaskText = string.Empty;
        TaskAdded?.Invoke(vm);
        NotifyChanged();
    }

    private void DeleteTask(TodoItemViewModel? item)
    {
        if (item == null) return;
        if (LinkedTask == item) LinkedTask = null;
        item.PropertyChanged -= OnItemPropertyChanged;
        Todos.Remove(item);
        NotifyChanged();
    }

    private void ToggleComplete(TodoItemViewModel? item)
    {
        if (item == null) return;
        var wasCompleted = item.IsCompleted;
        item.IsCompleted = !item.IsCompleted;
        if (item.IsCompleted)
        {
            item.CompletedDate = _todayKey;
            item.CompletedAt = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (LinkedTask == item) LinkedTask = null;
        }
        else
        {
            item.CompletedDate = null;
            item.CompletedAt = null;
        }

        TaskCompletionChanged?.Invoke(item, item.IsCompleted && !wasCompleted);
        NotifyChanged();
    }

    private void ClearCompleted()
    {
        for (int i = Todos.Count - 1; i >= 0; i--)
        {
            if (Todos[i].IsCompleted)
            {
                if (LinkedTask == Todos[i]) LinkedTask = null;
                Todos[i].PropertyChanged -= OnItemPropertyChanged;
                _archive.Add(Todos[i].Model);
                Todos.RemoveAt(i);
            }
        }
        NotifyChanged();
    }

    private void LinkToTimer(TodoItemViewModel? item)
    {
        if (item == null) return;
        LinkedTask = LinkedTask == item ? null : item;
        NotifyPlanProperties();
        NotifyChanged();
    }

    private void CyclePriority(TodoItemViewModel? item)
    {
        if (item == null) return;
        item.Priority = item.Priority switch
        {
            Priority.Low => Priority.Medium,
            Priority.Medium => Priority.High,
            _ => Priority.Low
        };
        NotifyChanged();
    }

    private void ChangeEstimate(TodoItemViewModel? item, int delta)
    {
        if (item == null) return;
        item.EstimatedPomodoros += delta;
        NotifyPlanProperties();
        NotifyChanged();
    }

    public int LoadFrom(IEnumerable<TodoItem> items, string todayKey)
    {
        _todayKey = todayKey;
        _archive.Clear();
        Todos.Clear();
        ReviewTasks.Clear();
        var carriedOver = 0;

        foreach (var item in items)
        {
            NormalizeDates(item);
            if (!item.IsCompleted && IsPastDate(item.PlannedForDate, _todayKey))
            {
                item.RolloverCount++;
                var vm = new TodoItemViewModel(item);
                WatchItem(vm);
                ReviewTasks.Add(vm);
                carriedOver++;
                continue;
            }

            if (!item.IsCompleted && IsFutureDate(item.PlannedForDate, _todayKey))
            {
                _archive.Add(item);
                NotifyTomorrowProperties();
                continue;
            }

            if (ShouldShow(item))
            {
                var vm = new TodoItemViewModel(item);
                WatchItem(vm);
                Todos.Add(vm);
            }
            else
            {
                _archive.Add(item);
            }
        }

        NotifyTomorrowProperties();
        NotifyChanged();
        return carriedOver;
    }

    public List<TodoItem> ToModels()
        => _archive
            .Concat(Todos.Select(t => t.Model))
            .Concat(ReviewTasks.Select(t => t.Model))
            .ToList();

    public void IncrementLinkedPomodoro()
    {
        if (LinkedTask != null)
        {
            LinkedTask.PomodoroCount++;
            NotifyChanged();
        }
    }

    public bool LinkFirstOpenTask()
    {
        var first = Todos.FirstOrDefault(t => !t.IsCompleted);
        if (first == null) return false;
        LinkedTask = first;
        NotifyPlanProperties();
        return true;
    }

    public bool LinkNextOpenTask()
    {
        var openTasks = Todos.Where(t => !t.IsCompleted).ToList();
        if (openTasks.Count == 0) return false;

        var currentIndex = LinkedTask == null ? -1 : openTasks.IndexOf(LinkedTask);
        LinkedTask = openTasks[(currentIndex + 1 + openTasks.Count) % openTasks.Count];
        NotifyPlanProperties();
        return true;
    }

    public bool CompleteLinkedTask()
    {
        if (LinkedTask == null || LinkedTask.IsCompleted) return false;
        ToggleComplete(LinkedTask);
        LinkFirstOpenTask();
        return true;
    }

    public int MoveOpenTasksToTomorrow()
    {
        var openTasks = Todos.Where(t => !t.IsCompleted).ToList();
        foreach (var item in openTasks)
        {
            if (LinkedTask == item)
                LinkedTask = null;

            item.PlannedForDate = TomorrowKey();
            item.PropertyChanged -= OnItemPropertyChanged;
            Todos.Remove(item);
            _archive.Add(item.Model);
        }

        if (openTasks.Count == 0)
            return 0;

        NotifyPlanProperties();
        NotifyTomorrowProperties();
        NotifyChanged();
        return openTasks.Count;
    }

    private void KeepReviewTask(TodoItemViewModel? item)
    {
        if (item == null) return;
        item.PlannedForDate = _todayKey;
        ReviewTasks.Remove(item);
        Todos.Insert(0, item);
        NotifyPlanProperties();
        NotifyReviewProperties();
        NotifyChanged();
    }

    private void DeferReviewTask(TodoItemViewModel? item)
    {
        if (item == null) return;
        item.PlannedForDate = TomorrowKey();
        item.PropertyChanged -= OnItemPropertyChanged;
        ReviewTasks.Remove(item);
        _archive.Add(item.Model);
        NotifyReviewProperties();
        NotifyTomorrowProperties();
        NotifyChanged();
    }

    private void DropReviewTask(TodoItemViewModel? item)
    {
        if (item == null) return;
        item.PropertyChanged -= OnItemPropertyChanged;
        ReviewTasks.Remove(item);
        NotifyReviewProperties();
        NotifyChanged();
    }

    private void KeepAllReviewTasks()
    {
        foreach (var item in ReviewTasks.ToList())
            KeepReviewTask(item);
    }

    private void DeferAllReviewTasks()
    {
        foreach (var item in ReviewTasks.ToList())
            DeferReviewTask(item);
    }

    private bool ShouldShow(TodoItem item)
        => (!item.IsCompleted && item.PlannedForDate == _todayKey)
            || item.CompletedDate == _todayKey;

    private static string TodayKey()
        => DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string TomorrowKey()
        => DateTime.Today.AddDays(1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static string DateKeyFromUnix(long unixMilliseconds)
        => DateTimeOffset.FromUnixTimeMilliseconds(unixMilliseconds)
            .LocalDateTime
            .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static void NormalizeDates(TodoItem item)
    {
        if (string.IsNullOrWhiteSpace(item.PlannedForDate))
            item.PlannedForDate = DateKeyFromUnix(item.CreatedAt);

        if (item.IsCompleted && string.IsNullOrWhiteSpace(item.CompletedDate) && item.CompletedAt.HasValue)
            item.CompletedDate = DateKeyFromUnix(item.CompletedAt.Value);
    }

    private static bool IsPastDate(string date, string todayKey)
        => string.CompareOrdinal(date, todayKey) < 0;

    private static bool IsFutureDate(string date, string todayKey)
        => string.CompareOrdinal(date, todayKey) > 0;

    private static bool IsTomorrowOpenTask(TodoItem item)
        => !item.IsCompleted && item.PlannedForDate == TomorrowKey();

    private void WatchItem(TodoItemViewModel item)
    {
        item.PropertyChanged -= OnItemPropertyChanged;
        item.PropertyChanged += OnItemPropertyChanged;
    }

    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(TodoItemViewModel.Text))
        {
            OnPropertyChanged(nameof(LinkedTaskText));
            NotifyChanged();
        }
        else if (e.PropertyName is nameof(TodoItemViewModel.Priority) or nameof(TodoItemViewModel.EstimatedPomodoros) or nameof(TodoItemViewModel.IsCompleted))
        {
            NotifyPlanProperties();
            NotifyChanged();
        }
    }

    private void NotifyChanged() => Changed?.Invoke();

    private void NotifyPlanProperties()
    {
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasTasks));
        OnPropertyChanged(nameof(HasOpenTasks));
        OnPropertyChanged(nameof(NeedsTopTask));
        OnPropertyChanged(nameof(OpenTasksCount));
        OnPropertyChanged(nameof(CompletedTasksCount));
        OnPropertyChanged(nameof(PlannedFocusBlocks));
        OnPropertyChanged(nameof(PlanSummaryText));
        OnPropertyChanged(nameof(TopTaskPromptText));
    }

    private void NotifyTomorrowProperties()
    {
        OnPropertyChanged(nameof(TomorrowTasksCount));
        OnPropertyChanged(nameof(TomorrowFocusBlocks));
        OnPropertyChanged(nameof(TomorrowPlanText));
    }

    private void NotifyReviewProperties()
    {
        OnPropertyChanged(nameof(HasReviewTasks));
        OnPropertyChanged(nameof(ReviewTasksCount));
        OnPropertyChanged(nameof(ReviewSummaryText));
    }
}
