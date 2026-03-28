# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

## [0.1.0] - 2026-03-28

### Added
- Todo list with full CRUD (create, read, update, delete)
- Priority levels (low, medium, high) with color-coded indicators
- Inline editing via double-click
- Pomodoro timer with configurable work/rest/total durations
- Circular SVG-style progress arc with phase-aware color transitions
- Timer-to-task linking with animated glow highlight on active task
- Frameless, always-on-top, draggable widget window
- System tray icon with context menu (show/hide, start/pause/reset, exit)
- Minimize to tray on close (exit via tray menu)
- Balloon notifications and system sound on phase changes
- Auto-save with 3-second debounce on every change
- Keyboard shortcuts (Ctrl+Space, Ctrl+R, Ctrl+E, Esc)
- JSON persistence to %AppData%/PomodoroWidget/
- Window position remembered between sessions
- Compact mode (timer only) and expanded mode (timer + todo list)
- Single-file portable exe publish (~71MB self-contained)
