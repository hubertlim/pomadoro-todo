using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PomodoroWidget.Services;
using PomodoroWidget.ViewModels;

namespace PomodoroWidget.Views;

public partial class MainWindow : Window
{
    private MainViewModel VM => (MainViewModel)DataContext;
    private TrayService? _tray;
    private bool _forceClose;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
        LocationChanged += (_, _) => VM.UpdatePosition(Left, Top);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var (left, top) = VM.GetSavedPosition();
        var screen = SystemParameters.WorkArea;
        Left = Math.Clamp(left, 0, screen.Width - 100);
        Top = Math.Clamp(top, 0, screen.Height - 100);
        VM.UpdatePosition(Left, Top);

        _tray = new TrayService(this, VM);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        VM.Save(Left, Top);
        if (!_forceClose)
        {
            e.Cancel = true;
            Hide();
            return;
        }
        _tray?.Dispose();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Hide();

    public void ForceClose()
    {
        _forceClose = true;
        Close();
    }

    // Keyboard shortcuts
    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        // Ctrl+Space: toggle start/pause
        if (e.Key == Key.Space && Keyboard.Modifiers == ModifierKeys.Control)
        {
            if (VM.Timer.IsRunning)
                VM.Timer.PauseCommand.Execute(null);
            else
                VM.Timer.StartCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+R: reset timer
        else if (e.Key == Key.R && Keyboard.Modifiers == ModifierKeys.Control)
        {
            VM.Timer.ResetCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+E: toggle expand
        else if (e.Key == Key.E && Keyboard.Modifiers == ModifierKeys.Control)
        {
            VM.ToggleExpandCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+P: return to plan
        else if (e.Key == Key.P && Keyboard.Modifiers == ModifierKeys.Control)
        {
            VM.ShowPlanCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+F: enter focus mode
        else if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control)
        {
            VM.BeginFocusCommand.Execute(null);
            e.Handled = true;
        }
        // Ctrl+Enter: choose the first open task and enter focus mode
        else if ((e.Key == Key.Enter || e.Key == Key.Return) && Keyboard.Modifiers == ModifierKeys.Control)
        {
            VM.SelectTopTaskAndFocusCommand.Execute(null);
            e.Handled = true;
        }
        // Enter in Focus: complete current task and move to the next open task
        else if (VM.IsFocusVisible && (e.Key == Key.Enter || e.Key == Key.Return)
                 && Keyboard.Modifiers == ModifierKeys.None
                 && e.OriginalSource is not TextBox)
        {
            VM.CompleteFocusedTaskCommand.Execute(null);
            e.Handled = true;
        }
        // N in Focus: switch to the next open task
        else if (VM.IsFocusVisible && e.Key == Key.N
                 && Keyboard.Modifiers == ModifierKeys.None
                 && e.OriginalSource is not TextBox)
        {
            VM.NextFocusTaskCommand.Execute(null);
            e.Handled = true;
        }
        // Escape: minimize to tray
        else if (e.Key == Key.Escape)
        {
            Hide();
            e.Handled = true;
        }
    }
}
