# Design Pass

## Review

- The widget had a clear product loop after the daily analytics work: plan today, focus with a linked task, complete work, and return tomorrow with unfinished tasks carried over.
- The visual system was still close to a prototype: saturated blue-purple accent, several hard-coded colors, corrupted icon glyphs in controls, and repeated card styling across views.
- The active task used an animated `DropShadowEffect` per list item. That looked lively, but blur animation in a scrollable WPF list is more expensive than the value it added.

## Changes

- Reworked the color system around neutral charcoal surfaces, teal focus accents, green rest states, amber medium priority, and red high priority.
- Added reusable `PanelCard`, `ProgressBar`, `CheckBox`, `ComboBox`, and `ScrollBar` styles in `Theme.xaml` so controls share the same component language.
- Replaced corrupted glyph labels with ASCII-safe labels to keep the app professional on systems with different fonts or encodings.
- Updated active task styling from a pulsing blur to a soft accent background and border.
- Replaced the native checkbox with a compact themed checkbox that matches the app's input surfaces and accent states.
- Kept lightweight entrance animation for new task rows and a single timer arc color animation. These are low-cost compared with per-row blur effects.
- Cached priority brushes in the converter and froze them, avoiding repeated brush allocation during binding refreshes.

## Direction

- Keep the widget calm and fast: one accent color, small motion, and clear hierarchy.
- Use progress surfaces for motivation, not decoration.
- Prefer reusable styles before adding one-off colors or borders in individual views.
- Move toward the Compact AI Console direction: black-glass status screen, small control deck, compact timer readout, and progressive disclosure for rare tools.
