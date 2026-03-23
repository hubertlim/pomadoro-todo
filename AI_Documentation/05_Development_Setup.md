# Development Setup Guide

## Prerequisites

### 1. Node.js (v18+)
```bash
# Verify installation
node --version
npm --version
```

### 2. Rust Toolchain
```bash
# Install via rustup (https://rustup.rs)
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh

# Verify
rustc --version
cargo --version
```

### 3. Windows Build Dependencies
- Microsoft Visual Studio C++ Build Tools (MSVC)
- WebView2 Runtime (pre-installed on Windows 10 1803+ and Windows 11)
- Install via Visual Studio Installer → "Desktop development with C++"

### 4. Tauri CLI
```bash
npm install -g @tauri-apps/cli@latest
```

## Project Initialization

```bash
# Create project with Tauri + SolidJS template
npm create tauri-app@latest pomodoro-widget -- --template template-solid-ts

# Navigate to project
cd pomodoro-widget

# Install dependencies
npm install

# Install Tauri plugins
npm install @tauri-apps/plugin-store @tauri-apps/plugin-notification
```

### Cargo dependencies (src-tauri/Cargo.toml)
```toml
[dependencies]
tauri = { version = "2", features = ["tray-icon"] }
tauri-plugin-store = "2"
tauri-plugin-notification = "2"
serde = { version = "1", features = ["derive"] }
serde_json = "1"
```

## Development Commands

```bash
# Start dev server with hot reload
npm run tauri dev

# Build production binary
npm run tauri build

# Check Rust code
cargo check --manifest-path src-tauri/Cargo.toml
```

## Tauri Configuration Highlights (tauri.conf.json)

```json
{
  "app": {
    "windows": [
      {
        "title": "Pomodoro Widget",
        "width": 380,
        "height": 600,
        "decorations": false,
        "alwaysOnTop": true,
        "transparent": true,
        "resizable": true,
        "minWidth": 300,
        "minHeight": 400
      }
    ],
    "withGlobalTauri": true
  }
}
```
