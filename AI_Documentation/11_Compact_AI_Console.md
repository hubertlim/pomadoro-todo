# Compact AI Console Direction

## Product Goal

The widget should feel like a small desktop work assistant, not a full productivity dashboard. Its job is to stay compact while keeping the loop visible:

1. Plan today.
2. Pick the next task.
3. Run focus blocks.
4. Finish or defer work.
5. Return tomorrow.

## Chosen Direction

Option B: Compact AI Console.

The visual language is inspired by a retro-future computer assistant: dark machine shell, black glass status screen, teal signal line, small waveform, and compact control deck. It should suggest an advanced work assistant without copying any specific character design.

## Implemented In First Pass

- Reduced the default widget size from `380 x 660` to `320 x 520`.
- Replaced the large top navigation row with a compact console status screen.
- Moved Plan, Focus, Stats, and End into a four-button control deck.
- Added a subtle WPF-native waveform pulse using opacity animation only.
- Compressed the timer into a two-column assistant readout.
- Kept timer settings available only in idle state.
- Tightened inner view margins for the smaller shell.
- Wrapped Shutdown in a scroll viewer so data tools remain available in compact mode.

## Performance Rules

- Use XAML-native shapes and text.
- Prefer opacity and transform animations only.
- Avoid animated blur, heavy shadow animation, particles, web views, or video.
- Keep visual effects global and sparse: one waveform pulse and existing timer arc color transition are enough.
- Continue using WPF and local files; no local services are required.

## Next Design Step

The next pass should compress task rows and move rare data tools behind a `More` or `Data` drawer so Shutdown feels less like a settings page.
