using System.Windows.Input;
using System.Windows.Threading;

namespace PomodoroWidget.ViewModels;

public enum Phase { Idle, Work, Rest }

public class TimerViewModel : ViewModelBase
{
    private readonly DispatcherTimer _timer;

    private int _workMinutes = 25;
    public int WorkMinutes { get => _workMinutes; set { SetProperty(ref _workMinutes, Math.Clamp(value, 1, 120)); if (CurrentPhase == Phase.Idle) Remaining = _workMinutes * 60; } }

    private int _restMinutes = 5;
    public int RestMinutes { get => _restMinutes; set => SetProperty(ref _restMinutes, Math.Clamp(value, 1, 60)); }

    private int _totalMinutes = 120;
    public int TotalMinutes { get => _totalMinutes; set { SetProperty(ref _totalMinutes, Math.Clamp(value, 1, 480)); OnPropertyChanged(nameof(MaxSessions)); } }

    private int _remaining;
    public int Remaining
    {
        get => _remaining;
        private set { SetProperty(ref _remaining, value); OnPropertyChanged(nameof(RemainingText)); OnPropertyChanged(nameof(Progress)); OnPropertyChanged(nameof(ArcEndAngle)); }
    }

    private int _totalElapsed;
    private int _sessionCount;
    public int SessionCount { get => _sessionCount; private set { SetProperty(ref _sessionCount, value); OnPropertyChanged(nameof(SessionText)); } }

    private Phase _currentPhase = Phase.Idle;
    public Phase CurrentPhase
    {
        get => _currentPhase;
        private set { SetProperty(ref _currentPhase, value); OnPropertyChanged(nameof(PhaseText)); OnPropertyChanged(nameof(IsIdle)); OnPropertyChanged(nameof(PhaseColor)); }
    }

    private bool _isRunning;
    public bool IsRunning { get => _isRunning; private set { SetProperty(ref _isRunning, value); OnPropertyChanged(nameof(IsIdle)); } }

    // Derived properties
    public string RemainingText => $"{Remaining / 60:D2}:{Remaining % 60:D2}";
    public string PhaseText => CurrentPhase.ToString().ToUpper();
    public string PhaseColor => CurrentPhase == Phase.Work ? "#6366F1" : CurrentPhase == Phase.Rest ? "#22C55E" : "#94A3B8";
    public bool IsIdle => CurrentPhase == Phase.Idle && !IsRunning;
    public int MaxSessions => (WorkMinutes + RestMinutes) > 0 ? TotalMinutes / (WorkMinutes + RestMinutes) : 0;
    public string SessionText => $"{SessionCount} / {MaxSessions}";

    public double Progress
    {
        get
        {
            int total = CurrentPhase == Phase.Work ? WorkMinutes * 60 : RestMinutes * 60;
            return total > 0 ? 1.0 - (double)Remaining / total : 0;
        }
    }

    public double ArcEndAngle => Progress * 360;

    public ICommand StartCommand { get; }
    public ICommand PauseCommand { get; }
    public ICommand ResetCommand { get; }

    public event Action? WorkPhaseCompleted;
    public event Action? TimerFinished;
    public event Action<Phase>? PhaseChanged;

    public TimerViewModel()
    {
        _remaining = _workMinutes * 60;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _timer.Tick += OnTick;

        StartCommand = new RelayCommand(_ => Start(), _ => !IsRunning);
        PauseCommand = new RelayCommand(_ => Pause(), _ => IsRunning);
        ResetCommand = new RelayCommand(_ => Reset());
    }

    private void Start()
    {
        if (CurrentPhase == Phase.Idle)
        {
            CurrentPhase = Phase.Work;
            Remaining = WorkMinutes * 60;
            _totalElapsed = 0;
            SessionCount = 0;
            PhaseChanged?.Invoke(Phase.Work);
        }
        IsRunning = true;
        _timer.Start();
    }

    private void Pause()
    {
        IsRunning = false;
        _timer.Stop();
    }

    public void Reset()
    {
        _timer.Stop();
        IsRunning = false;
        CurrentPhase = Phase.Idle;
        Remaining = WorkMinutes * 60;
        _totalElapsed = 0;
        SessionCount = 0;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Remaining--;
        _totalElapsed++;

        if (Remaining <= 0)
        {
            if (CurrentPhase == Phase.Work)
            {
                SessionCount++;
                WorkPhaseCompleted?.Invoke();
                CurrentPhase = Phase.Rest;
                Remaining = RestMinutes * 60;
                PhaseChanged?.Invoke(Phase.Rest);
            }
            else
            {
                CurrentPhase = Phase.Work;
                Remaining = WorkMinutes * 60;
                PhaseChanged?.Invoke(Phase.Work);
            }
        }

        if (_totalElapsed >= TotalMinutes * 60)
        {
            Reset();
            TimerFinished?.Invoke();
        }
    }
}
