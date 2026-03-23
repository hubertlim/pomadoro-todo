# Feature Specification

## 1. Todo List (CRUD)

### Create
- Text input + Add button (or Enter key)
- Optional priority: Low / Medium / High (color-coded)

### Read
- Scrollable list with visual distinction: active, pending, completed
- Active/current task highlighted with animated glow (DropShadowEffect + Storyboard)

### Update
- Double-click to inline edit task text
- Toggle completion via checkbox
- Cycle priority via click on priority indicator

### Delete
- Per-task delete button
- "Clear completed" bulk action

### Visual Effects
- Active task: pulsing glow border via DropShadowEffect with DoubleAnimation on Opacity/BlurRadius
- New task: SlideIn animation (TranslateTransform + OpacityAnimation)
- Completed task: FadeOut + Strikethrough
- Priority dots: Green (low), Yellow (medium), Red (high)

## 2. Pomodoro Timer

### Configuration
- Work duration (default 25 min)
- Rest/break duration (default 5 min)
- Total task time limit (end time for the session)

### Display
- Circular progress arc (WPF Arc geometry or ProgressBar with custom template)
- Digital countdown MM:SS
- Phase label: WORK / REST / IDLE
- Session counter: "Pomodoro 3 of 8"

### Controls
- Start / Pause / Reset buttons
- Auto-switch between work ↔ rest phases
- Stop when total time limit reached

### Task Integration
- Link timer to a specific todo item
- Linked task gets the glow highlight during work phase
- Track pomodoro count per task

### Notifications
- System tray balloon notification on phase change
- Optional audio beep via SystemSounds

## 3. Widget Window Behavior

- Frameless window (WindowStyle=None, AllowsTransparency=True)
- Custom title bar with drag support (DragMove)
- Always-on-top (Topmost=True)
- Compact mode (timer only) / Expanded mode (timer + todo list)
- System tray icon (NotifyIcon) with context menu
- Minimize to tray
- Remember window position between sessions
- Semi-transparent dark background with rounded corners
