# Architecture Overview

## High-Level Architecture

```
┌─────────────────────────────────────────┐
│              Tauri v2 Shell             │
│  ┌───────────────────────────────────┐  │
│  │         Rust Backend              │  │
│  │  - Window management              │  │
│  │  - System tray                    │  │
│  │  - Notifications                  │  │
│  │  - Data persistence (store)       │  │
│  └──────────────┬────────────────────┘  │
│                 │ IPC (invoke/listen)    │
│  ┌──────────────▼────────────────────┐  │
│  │     WebView2 (Windows)            │  │
│  │  ┌─────────────────────────────┐  │  │
│  │  │    SolidJS Frontend         │  │  │
│  │  │  ┌───────┐  ┌───────────┐  │  │  │
│  │  │  │ Todo  │  │ Pomodoro  │  │  │  │
│  │  │  │ CRUD  │  │  Timer    │  │  │  │
│  │  │  └───────┘  └───────────┘  │  │  │
│  │  │  ┌─────────────────────┐   │  │  │
│  │  │  │   Shared State      │   │  │  │
│  │  │  │  (SolidJS Signals)  │   │  │  │
│  │  │  └─────────────────────┘   │  │  │
│  │  └─────────────────────────────┘  │  │
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
```

## Project Structure

```
pomodoro-widget/
├── src/                      # SolidJS frontend
│   ├── components/
│   │   ├── TodoList.tsx      # Todo CRUD component
│   │   ├── TodoItem.tsx      # Single todo item with effects
│   │   ├── PomodoroTimer.tsx # Timer with circular progress
│   │   ├── TimerSettings.tsx # Work/rest/total time config
│   │   └── TitleBar.tsx      # Custom frameless title bar
│   ├── stores/
│   │   ├── todoStore.ts      # Todo state management (signals)
│   │   └── timerStore.ts     # Timer state management (signals)
│   ├── styles/
│   │   ├── global.css        # Base styles, CSS variables
│   │   ├── animations.css    # Glow, pulse, transitions
│   │   └── components/       # Per-component CSS modules
│   ├── App.tsx               # Root component
│   └── index.tsx             # Entry point
├── src-tauri/                # Rust backend
│   ├── src/
│   │   ├── main.rs           # Tauri app setup
│   │   ├── tray.rs           # System tray logic
│   │   └── commands.rs       # IPC commands
│   ├── Cargo.toml
│   └── tauri.conf.json       # Tauri configuration
├── index.html
├── package.json
├── tsconfig.json
├── vite.config.ts
└── AI_Documentation/         # This folder
```

## State Management

Using SolidJS signals (built-in reactivity, no external lib needed):

- `todoStore`: `createSignal` / `createStore` for todo list array
- `timerStore`: signals for countdown, phase, settings, linked task
- Persistence: serialize to Tauri store on every state change

## IPC Communication

- Frontend → Rust: `invoke("command_name", { args })` for system-level ops
- Rust → Frontend: `emit("event_name", payload)` for tray actions, notifications
