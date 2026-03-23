using System.Media;

namespace PomodoroWidget.Services;

public static class NotificationService
{
    private static System.Windows.Forms.NotifyIcon? _trayIcon;

    public static void SetTrayIcon(System.Windows.Forms.NotifyIcon icon)
        => _trayIcon = icon;

    public static void Notify(string title, string message)
    {
        _trayIcon?.ShowBalloonTip(3000, title, message, System.Windows.Forms.ToolTipIcon.Info);
        SystemSounds.Asterisk.Play();
    }

    public static void NotifyPhaseChange(string message)
        => Notify("🍅 Pomodoro Widget", message);
}
