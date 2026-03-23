using System.Windows;
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
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var (left, top) = VM.GetSavedPosition();
        var screen = SystemParameters.WorkArea;
        Left = Math.Clamp(left, 0, screen.Width - 100);
        Top = Math.Clamp(top, 0, screen.Height - 100);

        _tray = new TrayService(this, VM);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        VM.Save(Left, Top);
        if (!_forceClose)
        {
            // Minimize to tray instead of closing
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

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        // Minimize to tray
        Hide();
    }

    /// <summary>Called from tray "Exit" to truly close the app.</summary>
    public void ForceClose()
    {
        _forceClose = true;
        Close();
    }
}
