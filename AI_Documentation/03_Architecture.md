# Architecture Overview

## Pattern: MVVM (Model-View-ViewModel)

```
┌──────────────────────────────────────────┐
│            WPF Application               │
│                                          │
│  ┌─────────┐  Binding  ┌─────────────┐  │
│  │  Views   │◄────────►│ ViewModels   │  │
│  │  (XAML)  │          │  (C#)        │  │
│  └─────────┘          └──────┬──────┘  │
│                               │         │
│                        ┌──────▼──────┐  │
│                        │   Models     │  │
│                        │   + Services │  │
│                        └─────────────┘  │
└──────────────────────────────────────────┘
```

## Project Structure

```
PomodoroWidget/
├── PomodoroWidget.sln
├── PomodoroWidget/
│   ├── PomodoroWidget.csproj
│   ├── App.xaml / App.xaml.cs
│   ├── Models/
│   │   ├── TodoItem.cs
│   │   └── TimerSettings.cs
│   ├── ViewModels/
│   │   ├── ViewModelBase.cs        # INotifyPropertyChanged base
│   │   ├── MainViewModel.cs        # Root VM, orchestrates sub-VMs
│   │   ├── TodoViewModel.cs        # Todo CRUD logic
│   │   └── TimerViewModel.cs       # Pomodoro countdown logic
│   ├── Views/
│   │   ├── MainWindow.xaml         # Frameless widget shell
│   │   ├── TodoListView.xaml       # Todo list UserControl
│   │   ├── TodoItemView.xaml       # Single item template
│   │   ├── TimerView.xaml          # Circular timer UserControl
│   │   └── TimerSettingsView.xaml  # Work/rest/total config
│   ├── Services/
│   │   ├── DataService.cs          # JSON persistence (AppData)
│   │   └── NotificationService.cs  # Tray balloon + sound
│   ├── Converters/
│   │   └── PriorityToColorConverter.cs
│   ├── Styles/
│   │   ├── Theme.xaml              # Colors, brushes, base styles
│   │   └── Animations.xaml         # Storyboards, triggers
│   └── Assets/
│       └── icon.ico
└── AI_Documentation/
```

## Key Design Decisions

1. Single-window app: one `MainWindow` with `UserControl` panels
2. `DispatcherTimer` for the pomodoro countdown (UI-thread safe)
3. JSON file persistence via `System.Text.Json` in `%AppData%/PomodoroWidget/`
4. `System.Windows.Forms.NotifyIcon` for system tray (WPF has no built-in tray)
5. All animations in XAML Storyboards — no code-behind animation logic
6. `RelayCommand` (ICommand) for button bindings
