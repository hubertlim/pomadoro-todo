# Pomodoro Timer Logic

## Core Algorithm

```
STATE: { phase: WORK|REST, remaining: seconds, session: number, totalElapsed: seconds }

on START:
  set phase = WORK
  set remaining = workDuration
  start interval (1s tick)

on TICK:
  remaining -= 1
  totalElapsed += 1
  if remaining <= 0:
    if phase == WORK:
      session += 1
      notify("Work phase complete!")
      set phase = REST
      set remaining = restDuration
    else:
      notify("Break over! Back to work.")
      set phase = WORK
      set remaining = workDuration
  if totalElapsed >= totalTaskDuration:
    stop timer
    notify("Task time is up!")
```

## Configurable Parameters

| Parameter       | Default | Description                          |
|----------------|---------|--------------------------------------|
| workDuration   | 25 min  | Focus/work interval length           |
| restDuration   | 5 min   | Break interval length                |
| totalDuration  | 2 hours | Total time allocated for the task    |

## Session Calculation

```
maxSessions = floor(totalDuration / (workDuration + restDuration))
```

Example: 2h total, 25min work, 5min rest → 4 full pomodoro cycles

## Task Linking

- Each todo item can have an optional `linkedTimerId`
- When a pomodoro starts for a task, that task gets the `active` CSS class
- Session count is stored per task for productivity tracking
