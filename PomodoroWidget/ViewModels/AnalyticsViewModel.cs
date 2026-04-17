using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using PomodoroWidget.Models;

namespace PomodoroWidget.ViewModels;

public class AnalyticsViewModel : ViewModelBase
{
    private readonly List<DailyProgress> _progress = [];

    public ObservableCollection<DayProgressViewModel> Week { get; } = [];

    private string _todayKey = TodayKey();
    private int _dailyPomodoroGoal = 4;
    private int _currentStreak;
    private int _longestStreak;
    private string? _lastStreakDate;

    public int DailyPomodoroGoal
    {
        get => _dailyPomodoroGoal;
        set
        {
            var next = Math.Clamp(value, 1, 12);
            if (SetProperty(ref _dailyPomodoroGoal, next))
            {
                Refresh();
                CommandManager.InvalidateRequerySuggested();
                Changed?.Invoke();
            }
        }
    }

    public int CurrentStreak
    {
        get => _currentStreak;
        private set { if (SetProperty(ref _currentStreak, value)) OnPropertyChanged(nameof(StreakText)); }
    }

    public int LongestStreak
    {
        get => _longestStreak;
        private set => SetProperty(ref _longestStreak, value);
    }

    public string? LastStreakDate
    {
        get => _lastStreakDate;
        private set => SetProperty(ref _lastStreakDate, value);
    }

    public string StreakText => CurrentStreak == 1 ? "1 day" : $"{CurrentStreak} days";
    public int TodayPomodoros => Today.PomodorosCompleted;
    public int TodayTasksCreated => Today.TasksCreated;
    public int TodayTasksCompleted => Today.TasksCompleted;
    public int TodayFocusMinutes => Today.FocusMinutes;
    public int TodayCarriedOver => Today.TasksCarriedOver;
    public double TodayGoalPercent => Math.Min(100, (double)TodayPomodoros / DailyPomodoroGoal * 100);
    public string TodayGoalText => $"{TodayPomodoros}/{DailyPomodoroGoal} focus blocks";
    public string TodayPlanText => TodayTasksCreated == 1 ? "1 task planned" : $"{TodayTasksCreated} tasks planned";
    public string TodayDoneText => TodayTasksCompleted == 1 ? "1 task done" : $"{TodayTasksCompleted} tasks done";
    public string CheckInText => Today.CheckedIn ? "Streak saved today" : "Plan one task or finish one focus block";
    public string RolloverText => TodayCarriedOver == 0
        ? "No carry-over today"
        : TodayCarriedOver == 1
            ? "1 task carried into today"
            : $"{TodayCarriedOver} tasks carried into today";
    public int WeekFocusMinutes => LastSevenDays().Sum(d => d.FocusMinutes);
    public int WeekPomodoros => LastSevenDays().Sum(d => d.PomodorosCompleted);
    public int WeekTasksCompleted => LastSevenDays().Sum(d => d.TasksCompleted);
    public int WeekTasksCreated => LastSevenDays().Sum(d => d.TasksCreated + d.TasksCarriedOver);
    public int WeekCompletionPercent => WeekTasksCreated > 0
        ? (int)Math.Round((double)WeekTasksCompleted / WeekTasksCreated * 100)
        : 0;
    public int ActiveDaysCount => LastSevenDays().Count(day => day.CheckedIn || day.PomodorosCompleted > 0 || day.TasksCompleted > 0);
    public int AverageFocusMinutes => ActiveDaysCount > 0
        ? (int)Math.Round((double)WeekFocusMinutes / ActiveDaysCount)
        : 0;
    public string ConsistencyText => ActiveDaysCount == 1
        ? "1 active day"
        : $"{ActiveDaysCount}/7 active days";
    public string BestDayText
    {
        get
        {
            var best = LastSevenDays()
                .OrderByDescending(day => day.PomodorosCompleted)
                .ThenByDescending(day => day.TasksCompleted)
                .FirstOrDefault();

            if (best == null || (best.PomodorosCompleted == 0 && best.TasksCompleted == 0))
                return "No peak day yet";

            var date = DateTime.ParseExact(best.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var label = best.Date == _todayKey ? "Today" : date.ToString("ddd", CultureInfo.InvariantCulture);
            return $"{label}: {best.PomodorosCompleted} blocks, {best.TasksCompleted} done";
        }
    }
    public string WeeklyInsightText
    {
        get
        {
            if (!Today.CheckedIn) return "One small check-in protects the habit today.";
            if (TodayPomodoros < DailyPomodoroGoal) return "Start the next focus block before adding more tasks.";
            if (WeekCompletionPercent < 60 && WeekTasksCreated >= 3) return "Tomorrow will work better with a smaller list.";
            if (ActiveDaysCount >= 5) return "Strong week. Keep tomorrow's plan intentionally light.";
            return "Momentum is building. Repeat the same simple loop tomorrow.";
        }
    }
    public string MomentumText
    {
        get
        {
            if (!Today.CheckedIn) return "Small start, real momentum.";
            if (TodayPomodoros >= DailyPomodoroGoal) return "Daily focus goal complete.";
            if (TodayTasksCompleted > 0) return "Progress is moving.";
            return "The day is open. Pick the next task.";
        }
    }

    public ICommand IncreaseDailyGoalCommand { get; }
    public ICommand DecreaseDailyGoalCommand { get; }

    public event Action? Changed;

    private DailyProgress Today => EnsureDay(_todayKey);

    public AnalyticsViewModel()
    {
        IncreaseDailyGoalCommand = new RelayCommand(_ => DailyPomodoroGoal++, _ => DailyPomodoroGoal < 12);
        DecreaseDailyGoalCommand = new RelayCommand(_ => DailyPomodoroGoal--, _ => DailyPomodoroGoal > 1);
    }

    public void LoadFrom(
        IEnumerable<DailyProgress> progress,
        int dailyGoal,
        int currentStreak,
        int longestStreak,
        string? lastStreakDate)
    {
        _progress.Clear();
        _progress.AddRange(progress);
        DailyPomodoroGoal = dailyGoal;
        CurrentStreak = currentStreak;
        LongestStreak = longestStreak;
        LastStreakDate = lastStreakDate;

        NormalizeStreak();
        EnsureDay(_todayKey);
        Refresh();
    }

    public void StartToday(string todayKey, int carriedOver)
    {
        _todayKey = todayKey;
        var today = EnsureDay(todayKey);
        if (carriedOver > today.TasksCarriedOver)
            today.TasksCarriedOver = carriedOver;

        NormalizeStreak();
        RefreshAndSignal();
    }

    public List<DailyProgress> ToModels()
        => _progress
            .OrderBy(d => d.Date, StringComparer.Ordinal)
            .TakeLast(120)
            .ToList();

    public void RecordTaskCreated()
    {
        var today = Today;
        today.TasksCreated++;
        MarkCheckedIn(today);
        RefreshAndSignal();
    }

    public void RecordTaskCompletionChanged(bool isCompleted)
    {
        var today = Today;
        today.TasksCompleted = Math.Max(0, today.TasksCompleted + (isCompleted ? 1 : -1));
        if (isCompleted)
            MarkCheckedIn(today);
        RefreshAndSignal();
    }

    public void RecordPomodoro(int focusMinutes)
    {
        var today = Today;
        today.PomodorosCompleted++;
        today.FocusMinutes += Math.Max(1, focusMinutes);
        MarkCheckedIn(today);
        RefreshAndSignal();
    }

    private DailyProgress EnsureDay(string date)
    {
        var day = _progress.FirstOrDefault(d => d.Date == date);
        if (day != null) return day;

        day = new DailyProgress { Date = date };
        _progress.Add(day);
        return day;
    }

    private void MarkCheckedIn(DailyProgress day)
    {
        if (day.CheckedIn) return;

        day.CheckedIn = true;
        var yesterday = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (LastStreakDate == yesterday)
            CurrentStreak++;
        else if (LastStreakDate != day.Date)
            CurrentStreak = 1;

        LastStreakDate = day.Date;
        if (CurrentStreak > LongestStreak)
            LongestStreak = CurrentStreak;
    }

    private void NormalizeStreak()
    {
        if (string.IsNullOrWhiteSpace(LastStreakDate))
        {
            CurrentStreak = 0;
            return;
        }

        var yesterday = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (LastStreakDate != _todayKey && LastStreakDate != yesterday)
            CurrentStreak = 0;
    }

    private IEnumerable<DailyProgress> LastSevenDays()
    {
        for (var i = 6; i >= 0; i--)
        {
            var key = DateTime.Today.AddDays(-i).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            yield return EnsureDay(key);
        }
    }

    private void Refresh()
    {
        Week.Clear();
        foreach (var day in LastSevenDays())
        {
            var date = DateTime.ParseExact(day.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var label = date.Date == DateTime.Today ? "Today" : date.ToString("ddd", CultureInfo.InvariantCulture);
            Week.Add(new DayProgressViewModel(day, label, DailyPomodoroGoal, day.Date == _todayKey));
        }

        OnPropertyChanged(nameof(CurrentStreak));
        OnPropertyChanged(nameof(LongestStreak));
        OnPropertyChanged(nameof(StreakText));
        OnPropertyChanged(nameof(TodayPomodoros));
        OnPropertyChanged(nameof(TodayTasksCreated));
        OnPropertyChanged(nameof(TodayTasksCompleted));
        OnPropertyChanged(nameof(TodayFocusMinutes));
        OnPropertyChanged(nameof(TodayCarriedOver));
        OnPropertyChanged(nameof(TodayGoalPercent));
        OnPropertyChanged(nameof(TodayGoalText));
        OnPropertyChanged(nameof(TodayPlanText));
        OnPropertyChanged(nameof(TodayDoneText));
        OnPropertyChanged(nameof(CheckInText));
        OnPropertyChanged(nameof(RolloverText));
        OnPropertyChanged(nameof(WeekFocusMinutes));
        OnPropertyChanged(nameof(WeekPomodoros));
        OnPropertyChanged(nameof(WeekTasksCompleted));
        OnPropertyChanged(nameof(WeekTasksCreated));
        OnPropertyChanged(nameof(WeekCompletionPercent));
        OnPropertyChanged(nameof(ActiveDaysCount));
        OnPropertyChanged(nameof(AverageFocusMinutes));
        OnPropertyChanged(nameof(ConsistencyText));
        OnPropertyChanged(nameof(BestDayText));
        OnPropertyChanged(nameof(WeeklyInsightText));
        OnPropertyChanged(nameof(MomentumText));
    }

    private void RefreshAndSignal()
    {
        Refresh();
        Changed?.Invoke();
    }

    private static string TodayKey()
        => DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
