# Direction 2: Data Durability Foundation

## Constraint

No local services or applications are installed for this step. The app continues to use built-in .NET APIs and local files. If a future database or toolchain is needed, use Docker for supporting tools rather than installing services locally.

## Implemented

- Added `DataVersion` and `LastSavedAt` metadata to `AppData`.
- Added automatic backups before each save in `%AppData%/PomodoroWidget/backups/`.
- Added backup pruning so only the latest automatic backups are retained.
- Changed saves to write through a temporary file before replacing `data.json`.
- Added fallback loading from the latest backup if the main `data.json` cannot be read.
- Added a manual `Backup` action in the Shutdown screen.
- Added `Export` and `Import` actions in the Shutdown screen.
- Import creates a manual backup of the current data before applying the imported file.
- Added a Data Status panel in the Shutdown screen with data version, last save time, backup count, latest backup, and save-file path.
- Added `Restore latest` to restore the newest backup. The app captures which backup will be restored before making a safety backup of the current state.
- Hardened load/import normalization for missing lists, empty task text, invalid dates, out-of-range timers and goals, negative counters, invalid enum values, and invalid window positions.
- Added task CSV export for spreadsheet analysis.
- Added Markdown daily report export for readable end-of-day summaries.
- Added backup directory path to the Data Status panel.
- Deduplicates task IDs and merges duplicate daily progress records during load/import normalization.
- Added `Save now` and `Open folder` actions to the Shutdown data tools.
- Expanded Data Status with health, save size, latest backup size, and backup directory.
- `Move open to tomorrow` saves immediately after shifting the active open list into the next day's plan.

## Why This Comes Before SQLite

The app is still a compact WPF widget with a small data model. Versioned JSON snapshots deliver immediate safety without adding dependencies or migrations. A SQLite migration is still reasonable later, but now there is a safer bridge for real user data.

## Future Migration Path

1. Keep writing JSON backups for recovery.
2. Keep export-to-JSON so the user can leave the app cleanly.
3. Add a migration reader that imports `data.json` into SQLite if the data model outgrows JSON.
