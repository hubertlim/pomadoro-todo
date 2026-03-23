using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using PomodoroWidget.ViewModels;

namespace PomodoroWidget.Services;

public class TrayService : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private readonly Window _window;
    private readonly MainViewModel _vm;

    public TrayService(Window window, MainViewModel vm)
    {
        _window = window;
        _vm = vm;

        _trayIcon = new NotifyIcon
        {
            Icon = CreateIcon(),
            Text = "Pomodoro Widget",
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };

        _trayIcon.DoubleClick += (_, _) => ShowWindow();
        NotificationService.SetTrayIcon(_trayIcon);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();

        var showItem = new ToolStripMenuItem("Show/Hide", null, (_, _) => ToggleWindow());
        var startItem = new ToolStripMenuItem("Start Timer", null, (_, _) =>
            _window.Dispatcher.Invoke(() => _vm.Timer.StartCommand.Execute(null)));
        var pauseItem = new ToolStripMenuItem("Pause Timer", null, (_, _) =>
            _window.Dispatcher.Invoke(() => _vm.Timer.PauseCommand.Execute(null)));
        var resetItem = new ToolStripMenuItem("Reset Timer", null, (_, _) =>
            _window.Dispatcher.Invoke(() => _vm.Timer.ResetCommand.Execute(null)));
        var sep = new ToolStripSeparator();
        var exitItem = new ToolStripMenuItem("Exit", null, (_, _) =>
            _window.Dispatcher.Invoke(() =>
            {
                if (_window is Views.MainWindow mw) mw.ForceClose();
                else _window.Close();
            }));

        menu.Items.AddRange([showItem, new ToolStripSeparator(), startItem, pauseItem, resetItem, sep, exitItem]);
        return menu;
    }

    private void ShowWindow()
    {
        _window.Dispatcher.Invoke(() =>
        {
            _window.Show();
            _window.WindowState = WindowState.Normal;
            _window.Activate();
        });
    }

    private void ToggleWindow()
    {
        _window.Dispatcher.Invoke(() =>
        {
            if (_window.IsVisible)
                _window.Hide();
            else
                ShowWindow();
        });
    }

    /// <summary>
    /// Creates a simple 16x16 tomato-colored icon programmatically.
    /// No external .ico file needed.
    /// </summary>
    private static Icon CreateIcon()
    {
        using var bmp = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bmp);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(System.Drawing.Color.Transparent);
        // Tomato body
        using var brush = new SolidBrush(System.Drawing.Color.FromArgb(239, 68, 68));
        g.FillEllipse(brush, 2, 4, 12, 11);
        // Stem
        using var stemBrush = new SolidBrush(System.Drawing.Color.FromArgb(34, 197, 94));
        g.FillRectangle(stemBrush, 6, 1, 4, 4);
        return Icon.FromHandle(bmp.GetHicon());
    }

    public void Dispose()
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
    }
}
