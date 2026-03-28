# Contributing to Pomodoro Widget

Thanks for your interest in contributing! This project is open to everyone.

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR_USERNAME/pomodoro-widget.git`
3. Install .NET 9 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
4. Run the app: `dotnet run --project PomodoroWidget`

## Development Workflow

1. Create a branch from `main`: `git checkout -b feature/your-feature`
2. Make your changes
3. Test locally: `dotnet build PomodoroWidget`
4. Commit with a clear message (see below)
5. Push and open a Pull Request

## Commit Messages

We follow [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` — new feature
- `fix:` — bug fix
- `docs:` — documentation only
- `style:` — formatting, no code change
- `refactor:` — code restructuring
- `chore:` — build, tooling, dependencies

## Code Style

- Follow existing patterns in the codebase
- Use MVVM pattern: logic in ViewModels, UI in XAML
- Keep code-behind minimal (only UI event wiring)
- Use `ViewModelBase.SetProperty` for all bindable properties

## Reporting Bugs

Use the [Bug Report](https://github.com/YOUR_USERNAME/pomodoro-widget/issues/new?template=bug_report.md) template.

## Suggesting Features

Use the [Feature Request](https://github.com/YOUR_USERNAME/pomodoro-widget/issues/new?template=feature_request.md) template.

## Code of Conduct

Please read our [Code of Conduct](CODE_OF_CONDUCT.md) before participating.
