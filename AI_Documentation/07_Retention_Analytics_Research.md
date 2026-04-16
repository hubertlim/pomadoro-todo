# Retention and Analytics Notes

## Current App Review

- The app is a small WPF widget with a timer-first workflow, task CRUD, priority dots, task-to-timer linking, tray behavior, keyboard shortcuts, and JSON persistence in `%AppData%/PomodoroWidget/`.
- The task model previously tracked only task text, completion, priority, creation time, and pomodoro count. It did not know which day a task belonged to, when it was completed, whether it rolled over, or how much progress happened on a given day.
- The timer already raises `WorkPhaseCompleted`, which is the right point to record focus blocks and focus minutes.
- The todo list already owns task creation and completion, which is the right point to extend a daily habit streak.

## Duolingo Retention Patterns Applied

- Make the daily win small: Duolingo separated streak extension from larger daily goals, and reported better retention when one lesson could extend the streak while goal progress stayed separate. In this app, creating a task, completing a task, or finishing one focus block checks in for the day; the focus-block goal is tracked separately.
- Keep streaks visible: Duolingo treats streaks as a prominent, tangible signal of consistency. The dashboard now shows current streak and best streak.
- Use bite-sized sessions: Duolingo emphasizes quick sessions that fit into a day. Pomodoro focus blocks match that pattern, so the dashboard measures progress by completed focus blocks.
- Show progress, not just pressure: Duolingo combines points, streaks, milestones, and progress surfaces. The dashboard now shows planned tasks, completed tasks, carry-over, focus minutes, completion rate, and a 7-day trend.
- Be forgiving about unfinished work: Duolingo uses Streak Freeze to avoid demotivating users when life interrupts the habit. This app applies a lighter version by automatically rolling unfinished tasks into the new day instead of making the user rebuild the list from memory.

## Sources

- Duolingo, "Improving the streak: Forming habits one lesson at a time" - https://blog.duolingo.com/improving-the-streak/
- Duolingo, "The habit-building research behind your Duolingo streak" - https://blog.duolingo.com/how-duolingo-streak-builds-habit/
- Duolingo, "The Duolingo Method: 5 key principles that make learning fun and effective" - https://blog.duolingo.com/duolingo-teaching-method/
