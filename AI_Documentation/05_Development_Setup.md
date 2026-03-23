# Development Setup Guide

## Prerequisites

Only one thing needed: .NET SDK (9.0+)

```bash
# Verify
dotnet --version   # Should show 9.x
```

That's it. No other tools, runtimes, or package managers required.

## Project Creation

```bash
dotnet new wpf -n PomodoroWidget -o PomodoroWidget
dotnet new sln -n PomodoroWidget -o .
dotnet sln add PomodoroWidget/PomodoroWidget.csproj
```

## Development Commands

```bash
# Run in debug mode
dotnet run --project PomodoroWidget

# Build release
dotnet build -c Release

# Publish as single portable EXE (self-contained, no .NET install needed)
dotnet publish PomodoroWidget -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true

# Publish as small EXE (requires .NET on target machine)
dotnet publish PomodoroWidget -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true
```

## Project File Configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <UseWindowsForms>true</UseWindowsForms>  <!-- For NotifyIcon tray -->
    <ApplicationIcon>Assets\icon.ico</ApplicationIcon>
  </PropertyGroup>
</Project>
```

Note: `UseWindowsForms=true` is only for `System.Windows.Forms.NotifyIcon` (tray icon).
WPF has no built-in system tray control.

## Folder Setup

After project creation, add these folders:
- `Models/`
- `ViewModels/`
- `Views/`
- `Services/`
- `Converters/`
- `Styles/`
- `Assets/`
