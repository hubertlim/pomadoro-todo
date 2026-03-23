using System.Windows;
using System.Windows.Input;
using PomodoroWidget.ViewModels;

namespace PomodoroWidget.Views;

public partial class MainWindow : Window
{
    private MainViewModel VM => (MainViewModel)DataContext;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var (left, top) = VM.GetSavedPosition();
        // Clamp to screen bounds
        var screen = SystemParameters.WorkArea;
        Left = Math.Clamp(left, 0, screen.Width - 100);
        Top = Math.Clamp(top, 0, screen.Height - 100);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        VM.Save(Left, Top);
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 1) DragMove();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
