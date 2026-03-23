# Technology Stack Decision

## Chosen Stack: .NET 9 / WPF (C# + XAML)

### Why WPF

- Windows-native UI framework, ships with .NET — no external dependencies
- Hardware-accelerated rendering via DirectX — smooth animations out of the box
- Built-in Storyboard/animation system for glow, pulse, fade effects (GPU-composited)
- DropShadowEffect, BlurEffect, and ColorAnimation are first-class citizens
- MVVM pattern with built-in data binding — clean separation of concerns
- Frameless window (`WindowStyle="None"`, `AllowsTransparency="True"`) for widget look
- Can publish as a single self-contained `.exe` — zero install, fully portable
- .NET runtime is pre-installed on Windows 10/11; self-contained publish bundles it anyway

### Deployment: Single-File Portable EXE

```bash
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Result: one `.exe` file (~30-60MB self-contained) that runs on any Windows machine.
For machines with .NET already installed, framework-dependent publish yields ~1-2MB.

### Why NOT the alternatives

| Option | Rejected Because |
|--------|-----------------|
| Tauri + SolidJS | Requires Rust toolchain + Node.js to build; WebView2 dependency |
| Electron | 100MB+ bundle, ships Chromium, heavy RAM usage |
| WinForms | Limited animation support, dated rendering pipeline |
| WinUI 3 | Requires Windows App SDK install, more complex deployment |
| UWP | Store-only distribution, sandboxed, limited window control |
| Pure Win32/C | Maximum effort for UI, no built-in animation framework |

### Architecture Pattern: MVVM

- Model: `TodoItem`, `PomodoroSession` data classes
- ViewModel: `TodoViewModel`, `TimerViewModel` with `INotifyPropertyChanged`
- View: XAML with data binding, styles, and storyboard animations
- Data persistence: JSON file in `AppData` via `System.Text.Json`
