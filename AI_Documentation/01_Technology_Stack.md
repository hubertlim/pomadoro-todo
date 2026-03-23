# Technology Stack Decision

## Chosen Stack: Tauri v2 + SolidJS + TypeScript

### Why Tauri v2 (Backend / Desktop Shell)

- Uses the OS native webview (WebView2 on Windows) instead of bundling Chromium
- Rust backend: memory-safe, high performance, no garbage collector overhead
- Binary size as low as ~600KB–8MB vs Electron's 100MB+
- ~90% less memory usage than Electron ([source](https://markaicode.com/tauri-vs-electron-desktop-app-framework-comparison/))
- Sub-500ms startup time vs Electron's 1–2 seconds ([source](https://www.raftlabs.com/blog/tauri-vs-electron-pros-cons/))
- Native system tray, always-on-top window, and transparent/frameless window support
- Built-in auto-updater, notifications, and file system access
- Tauri v2 supports both desktop and mobile targets

### Why SolidJS (Frontend UI)

- Fine-grained reactivity: updates only the exact DOM nodes that changed (no virtual DOM diffing)
- Compiles to direct DOM manipulations — smallest runtime overhead among modern frameworks
- Benchmarks consistently show SolidJS as the fastest JS framework ([source](https://www.frontendtools.tech/blog/best-frontend-frameworks-2025-comparison))
- JSX syntax (familiar to React devs) but with true reactivity
- Tiny bundle size (~7KB gzipped)
- Ideal for a widget where every KB and ms matters

### Why TypeScript

- Type safety for both frontend logic and Tauri command interfaces
- Better DX with autocompletion and compile-time error checking

### Styling: CSS Modules + CSS Animations

- No runtime CSS-in-JS overhead
- Native CSS animations for glow/pulse/highlight effects (GPU-accelerated)
- CSS `@keyframes` for pomodoro progress ring and task highlight effects

### Data Persistence: Tauri Store Plugin / localStorage

- `@tauri-apps/plugin-store` for lightweight JSON-based local storage
- No need for SQLite for a simple todo + pomodoro widget

## Alternatives Considered

| Framework | Rejected Because |
|-----------|-----------------|
| Electron  | 100MB+ bundle, high RAM, ships full Chromium |
| React     | Virtual DOM overhead, larger runtime than SolidJS |
| Svelte    | Close competitor, but SolidJS edges it in raw DOM update benchmarks |
| Vue       | Heavier runtime, virtual DOM |
| WinUI 3   | C#/XAML only, no web tech reuse, steeper learning curve |
