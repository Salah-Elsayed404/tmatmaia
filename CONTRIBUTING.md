# Contributing to Tmatmaia

Thanks for helping out.

## Getting set up

1. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) on Windows 11.
2. Clone the repo and run `dotnet build` from the root.
3. Run the app with `dotnet run --project src/Tmatmaia`.

Only one instance runs at a time, so close a running Tmatmaia (tray icon → Exit) before starting a new build.

## Where things live

- Timer logic goes in `Core/PomodoroEngine.cs`. Keep it free of UI code.
- New settings: add the property to `Core/Settings.cs`, a control to `Views/SettingsWindow.xaml`, and load/save it in `SettingsWindow.xaml.cs`.
- Anything shown in the taskbar goes in `Views/TaskbarWidget.xaml`.
- Design decisions and limitations are in `docs/DESIGN.md`. Update it when you change behavior.

## Style

- Follow `.editorconfig`: file-scoped namespaces, `_field` for private fields, `s_field` for private static fields.
- Keep pull requests focused on one change and describe how you tested it. Mention your Windows build and display scaling for anything touching the taskbar.

## Reporting bugs

Include your Windows version (`winver`), display scaling, taskbar alignment (left or center), and whether you use one or several monitors.
