# Visual Effects Guide

## Task Highlight Effects

### Active Task Glow
```css
@keyframes task-glow {
  0%, 100% { box-shadow: 0 0 5px rgba(99, 102, 241, 0.4); }
  50%      { box-shadow: 0 0 20px rgba(99, 102, 241, 0.8), 0 0 40px rgba(99, 102, 241, 0.3); }
}

.task-active {
  animation: task-glow 2s ease-in-out infinite;
  border: 1px solid rgba(99, 102, 241, 0.6);
}
```

### Task Enter/Exit Transitions
```css
.task-enter {
  animation: slideIn 0.3s ease-out forwards;
}

@keyframes slideIn {
  from { opacity: 0; transform: translateY(-10px) scale(0.95); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

.task-exit {
  animation: slideOut 0.3s ease-in forwards;
}

@keyframes slideOut {
  from { opacity: 1; transform: translateX(0); }
  to   { opacity: 0; transform: translateX(100px); }
}
```

### Priority Color Indicators
```css
:root {
  --priority-low: #22c55e;     /* green */
  --priority-medium: #eab308;  /* yellow */
  --priority-high: #ef4444;    /* red */
}
```

## Pomodoro Timer Effects

### Circular Progress Ring (SVG approach)
- SVG circle with `stroke-dasharray` and `stroke-dashoffset`
- Animate `stroke-dashoffset` based on remaining time
- Color transitions: work phase (indigo) → rest phase (emerald)

### Phase Transition
```css
@keyframes phase-switch {
  0%   { transform: scale(1); filter: brightness(1); }
  50%  { transform: scale(1.05); filter: brightness(1.3); }
  100% { transform: scale(1); filter: brightness(1); }
}
```

## Widget Window Effects

### Glassmorphism Background
```css
.widget-container {
  background: rgba(15, 15, 25, 0.85);
  backdrop-filter: blur(12px);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 16px;
}
```

## Performance Notes

- All animations use `transform` and `opacity` only (GPU-composited, no layout thrashing)
- Use `will-change: transform` sparingly on animated elements
- Prefer CSS animations over JS-driven animations for widget-level effects
- `requestAnimationFrame` only for the timer countdown display update
