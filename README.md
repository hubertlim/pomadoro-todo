# Pomodoro Widget

A minimalistic, high-performance Windows desktop widget combining a todo list with a Pomodoro timer.

## Features

- **Todo List (CRUD)** — create, edit, complete, and delete tasks with priority levels
- **Visual Highlighting** — active tasks glow with animated effects; smooth enter/exit transitions
- **Pomodoro Timer** — configurable work/rest intervals with a total task duration limit
- **Circular Progress** — SVG-based countdown ring with phase-aware color transitions
- **Desktop Widget** — frameless, always-on-top, draggable window with system tray integration
- **Compact & Expanded Modes** — toggle between timer-only and full view

## Tech Stack

| Layer    | Technology       | Why                                              |
|----------|-----------------|--------------------------------------------------|
| Shell    | Tauri v2         | Native webview, ~600KB binary, Rust backend      |
| Frontend | SolidJS + TS     | Fine-grained reactivity, no virtual DOM, ~7KB    |
| Styling  | CSS Modules      | Zero runtime cost, GPU-accelerated animations    |
| Storage  | Tauri Store      | Lightweight JSON persistence, no DB overhead     |

## Prerequisites

- Node.js 18+
- Rust toolchain ([rustup.rs](https://rustup.rs))
- MSVC Build Tools (Visual Studio Installer → "Desktop development with C++")
- WebView2 Runtime (included in Windows 10 1803+ / Windows 11)

## Quick Start

```bash
# Install dependencies
npm install

# Run in development mode
npm run tauri dev

# Build production binary
npm run tauri build
```

## Project Structure

```
pomodoro-widget/
├── src/                    # SolidJS frontend
│   ├── components/         # UI components (TodoList, PomodoroTimer, etc.)
│   ├── stores/             # State management (SolidJS signals)
│   └── styles/             # CSS modules and animations
├── src-tauri/              # Rust backend (window, tray, persistence)
├── AI_Documentation/       # Design docs, specs, and research
└── index.html
```

## Documentation

Detailed design and implementation docs are in [`AI_Documentation/`](./AI_Documentation/):

| Document | Contents |
|----------|----------|
| [01_Technology_Stack](./AI_Documentation/01_Technology_Stack.md) | Stack decision rationale and alternatives |
| [02_Feature_Specification](./AI_Documentation/02_Feature_Specification.md) | Full feature spec for todo + pomodoro |
| [03_Architecture](./AI_Documentation/03_Architecture.md) | System architecture and project structure |
| [04_Visual_Effects_Guide](./AI_Documentation/04_Visual_Effects_Guide.md) | CSS animations and effect recipes |
| [05_Development_Setup](./AI_Documentation/05_Development_Setup.md) | Environment setup and build instructions |
| [06_Pomodoro_Logic](./AI_Documentation/06_Pomodoro_Logic.md) | Timer algorithm and session management |

## License

MIT
