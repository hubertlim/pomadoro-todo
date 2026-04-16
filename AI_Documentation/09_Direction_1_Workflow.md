# Direction 1: Guided Daily Planning And Focus

## Goal

Build the core loop first:

1. Plan today.
2. Focus on one task with the timer.
3. Review the day.
4. Carry unfinished work forward tomorrow.

## Duplicate Work Avoided

- The todo list remains the planning surface. A separate planner would duplicate task creation, priority, estimates, and rollover logic.
- The timer remains a single global surface at the top of the widget. Focus mode references it instead of embedding another timer.
- The dashboard remains the analytics surface. Shutdown mode summarizes the day without recreating charts.
- Task rollover stays in `TodoViewModel.LoadFrom`, so shutdown does not need its own carry-over implementation.

## Implemented

- Added a workflow shell in `MainViewModel` with Plan, Focus, Stats, and Shutdown surfaces.
- Added Focus mode for the currently linked task, with actions for start, pause, done, next task, and return to plan.
- Added Shutdown mode with a simple end-of-day summary and tomorrow carry-over reminder.
- Added per-task focus-block estimates and plan capacity guidance.
- Reused the existing first-run empty planning form as the start of the daily ritual.
- Added Morning Review for unfinished tasks from previous days, with choices to keep today, defer until tomorrow, or drop.
- Added a top-task prompt in the planning surface so the user deliberately chooses the first focus target after review or initial planning.
- Added keyboard-first flow: Ctrl+Enter chooses the first open task and enters Focus, Ctrl+F enters Focus, Ctrl+P returns to Plan, Enter completes in Focus, and N switches to the next open task.

## Next Step

The next high-leverage improvement is data durability:

- Move persistence from one JSON file to a small local database or versioned JSON snapshots.
- Add backup/export so users trust the app for real work.
- Add a migration path before building more advanced workflows.
