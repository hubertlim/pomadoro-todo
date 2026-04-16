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

## Why This Comes Before SQLite

The app is still a compact WPF widget with a small data model. Versioned JSON snapshots deliver immediate safety without adding dependencies or migrations. A SQLite migration is still reasonable later, but now there is a safer bridge for real user data.

## Future Migration Path

1. Keep writing JSON backups for recovery.
2. Add a migration reader that imports `data.json` into SQLite.
3. Keep export-to-JSON so the user can leave the app cleanly.
