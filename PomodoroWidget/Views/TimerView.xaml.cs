using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PomodoroWidget.ViewModels;

namespace PomodoroWidget.Views;

public partial class TimerView : UserControl
{
    private static readonly Color WorkColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#6366F1");
    private static readonly Color RestColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#22C55E");
    private static readonly Color IdleColor = (Color)System.Windows.Media.ColorConverter.ConvertFromString("#94A3B8");

    public TimerView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is TimerViewModel oldVm)
            oldVm.PropertyChanged -= OnTimerPropertyChanged;
        if (e.NewValue is TimerViewModel newVm)
            newVm.PropertyChanged += OnTimerPropertyChanged;
    }

    private void OnTimerPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(TimerViewModel.CurrentPhase)) return;
        if (sender is not TimerViewModel vm) return;

        var target = vm.CurrentPhase switch
        {
            Phase.Work => WorkColor,
            Phase.Rest => RestColor,
            _ => IdleColor
        };

        var anim = new ColorAnimation(target, TimeSpan.FromMilliseconds(400))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
        };
        ArcBrush.BeginAnimation(SolidColorBrush.ColorProperty, anim);
    }
}
