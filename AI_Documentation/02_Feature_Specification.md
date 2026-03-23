# Feature Specification

## 1. Todo List (CRUD)

### Create
- Input field with "Add" button or Enter key
- Optional priority level (low / medium / high)

### Read
- Scrollable list of all tasks
- Visual distinction between active, completed, and in-progress tasks
- Current/active task highlighted with a glow/pulse animation

### Update
- Inline editing on double-click
- Toggle completion status (checkbox)
- Change priority level

### Delete
- Delete button per task (with confirmation or undo toast)
- Bulk clear completed tasks

### Visual Effects for Current Task
- Pulsing glow border (CSS `box-shadow` animation) on the active task
- Subtle scale-up animation when a task becomes active
- Color-coded priority indicators (green/yellow/red)
- Smooth enter/exit transitions using CSS transitions

## 2. Pomodoro Timer

### Core Functionality
- Configurable work period (default: 25 min)
- Configurable rest/break period (default: 5 min)
- Configurable total task duration (end time)
- Start / Pause / Reset controls
- Auto-switch between work and rest phases

### Timer Display
- Circular progress ring (SVG or CSS conic-gradient)
- Digital countdown (MM:SS)
- Phase indicator (WORK / REST)
- Session counter (e.g., "Pomodoro 3/8")

### Task Integration
- Link a pomodoro session to a specific todo task
- Auto-highlight the linked task during work phase
- Track total pomodoro sessions per task

### Notifications
- System notification when work/rest phase ends (via Tauri notification plugin)
- Optional sound alert

## 3. Widget Behavior

### Window Properties
- Frameless, always-on-top window
- Draggable title bar area
- Resizable with min/max constraints
- System tray icon with quick actions (show/hide, start/pause timer)
- Transparent/semi-transparent background option

### Layout
- Compact mode: timer only (small floating widget)
- Expanded mode: timer + todo list
- Toggle between modes via button or tray menu
