<div align="center">

# 🍅 Pomodoro Widget

A tiny, native Windows desktop widget that keeps your tasks and focus timer always visible.

No browser. No Electron. No install. Just one `.exe`.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/Platform-Windows-0078D6.svg)](#)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](CONTRIBUTING.md)

<!-- 
  TODO: Replace with an actual screenshot or GIF of the running widget.
  A 3-5 second GIF showing: add task → start timer → glow effect is ideal.
  
  ![Pomodoro Widget Screenshot](docs/screenshot.png)
-->

*Screenshot coming soon — run it yourself in 30 seconds (see below)*

</div>

---

## Why?

Most productivity apps are heavy, full-screen, or browser-based. This is the opposite:

- A small floating window that stays on top of everything
- Frameless, draggable, semi-transparent — feels like a native OS widget
- Pomodoro timer linked to your tasks with a visual glow on the active one
- Runs from a single portable `.exe` (~71MB) — no installation, no runtime needed
- Built with WPF + .NET 9 — native Windows rendering, GPU-accelerated animations

## Download

> Head to [**Releases**](https://github.com/YOUR_USERNAME/pomodoro-widget/releases) and grab the latest `PomodoroWidget.exe`. Double-click to run. That's it.

Or build from source:

```bash
git clone https://github.com/YOUR_USERNAME/pomodoro-widget.git
cd pomodoro-widget
dotnet run --project PomodoroWidget
```

## Features

| Feature | Details |
|---------|---------|
| 📝 Todo CRUD | Add, edit (double-click), complete, delete, priority levels |
| ✨ Active task glow | Pulsing animated highlight on the task linked to the timer |
| ⏱ Pomodoro timer | Configurable work/rest/total time, circular progress arc |
| 🔔 Notifications | System tray balloon + sound on every phase change |
| 📌 Always on top | Frameless, draggable, semi-transparent floating window |
| 🔽 Tray minimize | Close button minimizes to tray; exit via tray context menu |
| 💾 Auto-save | Debounced save on every change — crash-safe |
| ⌨️ Shortcuts | Ctrl+Space (start/pause), Ctrl+R (reset), Ctrl+E (toggle), Esc (hide) |
| 📦 Portable | Single `.exe`, no install, no dependencies on target machine |

## Tech Stack

| Layer | Technology | Why |
|-------|-----------|-----|
| Framework | .NET 9 / WPF | Native Windows, GPU-accelerated, built-in animation system |
| Language | C# + XAML | Strong typing, MVVM data binding, mature ecosystem |
| Animations | WPF Storyboards | Hardware-accelerated on the composition thread |
| Persistence | System.Text.Json | Lightweight JSON to `%AppData%`, no database |
| Tray | WinForms NotifyIcon | Only reliable system tray solution for WPF |

## Project Structure

```
PomodoroWidget/
├── Models/          # Data models (TodoItem, AppData)
├── ViewModels/      # MVVM logic (MainVM, TodoVM, TimerVM)
├── Views/           # XAML UI (MainWindow, TodoList, Timer)
├── Services/        # Persistence, notifications, system tray
├── Converters/      # Value converters (priority→color, angle→arc)
├── Styles/          # Theme colors, button/textbox styles
└── GlobalUsings.cs  # Namespace disambiguation (WPF vs WinForms)
```

## Building from Source

Prerequisites: [.NET 9 SDK](https://dotnet.microsoft.com/download)

```bash
# Development
dotnet run --project PomodoroWidget

# Release build
dotnet build PomodoroWidget -c Release

# Publish portable exe (self-contained, ~71MB)
dotnet publish PomodoroWidget -c Release

# Publish lightweight exe (requires .NET on target, ~200KB)
dotnet publish PomodoroWidget -c Release --self-contained false
```

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+Space` | Start / Pause timer |
| `Ctrl+R` | Reset timer |
| `Ctrl+E` | Toggle todo list |
| `Esc` | Minimize to tray |

## Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) before submitting a PR.

See the [open issues](https://github.com/YOUR_USERNAME/pomodoro-widget/issues) for a list of known issues and feature ideas.

## Roadmap

- [ ] Custom themes / color schemes
- [ ] Drag-and-drop task reordering
- [ ] Task categories / tags
- [ ] Statistics dashboard (daily/weekly pomodoro count)
- [ ] Global hotkeys (work even when widget is not focused)
- [ ] Optional startup with Windows
- [ ] Sound customization

## License

[MIT](LICENSE) — use it however you want.
