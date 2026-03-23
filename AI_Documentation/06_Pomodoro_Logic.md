# Pomodoro Timer Logic

## Core State Machine

```
States: IDLE → WORK → REST → WORK → REST → ... → DONE

IDLE:  Timer not started. Shows configured work duration.
WORK:  Counting down workDuration. Active task glows.
REST:  Counting down restDuration. Glow paused.
DONE:  Total time limit reached. Timer stops.
```

## Algorithm (C# DispatcherTimer)

```csharp
// On each 1-second tick:
Remaining--;
TotalElapsed++;

if (Remaining <= 0)
{
    if (Phase == Phase.Work)
    {
        SessionCount++;
        Phase = Phase.Rest;
        Remaining = RestDuration;
        NotifyPhaseChange("Work complete! Take a break.");
    }
    else
    {
        Phase = Phase.Work;
        Remaining = WorkDuration;
        NotifyPhaseChange("Break over! Focus time.");
    }
}

if (TotalElapsed >= TotalDuration)
{
    Stop();
    NotifyPhaseChange("Task time is up!");
}
```

## Configurable Parameters

| Parameter     | Default | Range     | Description                    |
|--------------|---------|-----------|--------------------------------|
| WorkDuration | 25 min  | 1–120 min | Focus interval                 |
| RestDuration | 5 min   | 1–60 min  | Break interval                 |
| TotalDuration| 120 min | 1–480 min | Total allocated time for task  |

## Session Calculation

```
MaxSessions = Floor(TotalDuration / (WorkDuration + RestDuration))
```

Example: 120min total, 25min work, 5min rest → 4 full pomodoro cycles

## Task Linking

- `TimerViewModel.LinkedTaskId` binds to a `TodoItem.Id`
- When timer is in WORK phase, `TodoItem.IsActive = true` → triggers glow
- On each completed work phase, `TodoItem.PomodoroCount++`
- Unlinking clears the glow

## Persistence

Timer settings saved to `%AppData%/PomodoroWidget/settings.json`
Todo list saved to `%AppData%/PomodoroWidget/todos.json`
Window position saved to `%AppData%/PomodoroWidget/window.json`
