# Pomodoro Widget

A minimalistic, native Windows desktop widget combining a todo list with a Pomodoro timer. Runs as a small, movable, always-on-top window — no browser, no web runtime, no external dependencies.

## Features

- Todo list with full CRUD — add, edit, complete, delete tasks with priority levels
- Active task glow — animated pulsing highlight on the current task (WPF DropShadowEffect)
- Pomodoro timer — configurable work/rest intervals with total task time limit
- Circular progress arc — visual countdown with phase-aware color transitions
- Desktop widget — frameless, always-on-top, draggable, semi-transparent window
- System tray — minimize to tray, quick actions via context menu
- Compact / expanded mode — toggle between timer-only and full view
- Portable — publishes as a single `.exe` (~71MB self-contained), zero install required
- Auto-save — debounced persistence on every change, crash-safe
- Keyboard shortcuts — Ctrl+Space start/pause, Ctrl+R reset, Ctrl+E toggle, Esc minimize

## Tech Stack

| Layer       | Technology     | Why                                           |
|-------------|---------------|-----------------------------------------------|
| Framework   | .NET 9 / WPF  | Native Windows, GPU-accelerated, built-in animations |
| Language    | C# + XAML     | Strong typing, MVVM data binding              |
| Animations  | WPF Storyboards | Hardware-accelerated, composition thread      |
| Persistence | System.Text.Json | Lightweight JSON to AppData, no DB needed   |
| Tray Icon   | WinForms NotifyIcon | Only reliable tray solution for WPF       |

## Prerequisites

- .NET 9 SDK ([download](https://dotnet.microsoft.com/download))

That's it. Nothing else to install.

## Quick Start

```bash
# Run in development
dotnet run --project PomodoroWidget

# Publish portable single-file EXE (self-contained, ~71MB)
dotnet publish PomodoroWidget -c Release

# Publish framework-dependent (requires .NET on target, ~200KB)
dotnet publish PomodoroWidget -c Release --self-contained false
```

## Keyboard Shortcuts

| Shortcut      | Action              |
|---------------|---------------------|
| Ctrl+Space    | Start / Pause timer |
| Ctrl+R        | Reset timer         |
| Ctrl+E        | Toggle todo list    |
| Esc           | Minimize to tray    |

## Project Structure

```
PomodoroWidget/
├── Models/          # TodoItem, TimerSettings
├── ViewModels/      # MVVM logic (MainVM, TodoVM, TimerVM)
├── Views/           # XAML UI (MainWindow, TodoList, Timer)
├── Services/        # JSON persistence, notifications
├── Converters/      # Value converters (priority → color)
├── Styles/          # Theme, animations, storyboards
└── Assets/          # App icon
```

## Documentation

Design docs in [`AI_Documentation/`](./AI_Documentation/):

| Document | Contents |
|----------|----------|
| [01_Technology_Stack](./AI_Documentation/01_Technology_Stack.md) | Stack decision and alternatives |
| [02_Feature_Specification](./AI_Documentation/02_Feature_Specification.md) | Full feature spec |
| [03_Architecture](./AI_Documentation/03_Architecture.md) | MVVM architecture and project layout |
| [04_Visual_Effects_Guide](./AI_Documentation/04_Visual_Effects_Guide.md) | WPF animation recipes |
| [05_Development_Setup](./AI_Documentation/05_Development_Setup.md) | Build and publish commands |
| [06_Pomodoro_Logic](./AI_Documentation/06_Pomodoro_Logic.md) | Timer algorithm and state machine |

## License

MIT
