# Tmatmaia — Design

A Pomodoro timer for Windows 11 that sits inside the empty area of the taskbar.

## Decisions

| Topic | Decision | Why |
|---|---|---|
| Stack | C# / .NET 10 / WPF | Native Windows UI. Deskband11Lib.Wpf targets `net10.0-windows10.0.26100.0`, so the app does too. |
| Taskbar placement | [Deskband11Lib](https://github.com/airtaxi/Deskband11Lib) reparents the window into `Shell_TrayWnd` and uses UI Automation to find free space | Windows 11 removed DeskBands. A real child of the taskbar is never covered when the taskbar is clicked, unlike a topmost overlay. The library handles Explorer restarts, DPI changes and secondary monitors. |
| Fallback | `Overlay` placement: a borderless topmost window over the taskbar | Keeps the app usable if a Windows update breaks the embedding. |
| Base project | Built from scratch | The closest existing project, taskbar-focus (Rust), is a tray icon plus an optional floating window, not a taskbar widget. |
| Settings storage | JSON in `%APPDATA%\Tmatmaia\settings.json` | Simple and easy to back up. |
| Icon | Drawn from code by `scripts/make-icon.ps1` | Anyone can tweak and regenerate it without design tools. |

## Behavior

**Taskbar widget** (250×48)
- Large `mm:ss` time with the phase (`Focus #n`, `Break`, `Long break`) beside it, and a thin progress bar (red for focus, green for breaks).
- ▶/⏸ start/pause and ⟲ reset the current phase.
- Single click on the time toggles start/pause after the system double-click interval; a double click opens Settings instead.

**Timer engine** (`Core/PomodoroEngine.cs`)
- Phases: `Focus → ShortBreak | LongBreak → Focus…`. A long break follows every N completed focus phases.
- Remaining time is computed from an end timestamp, so sleep and timer drift don't skew it. A 250 ms `DispatcherTimer` only refreshes the UI.
- Changing durations applies immediately when paused, otherwise from the next phase.

**App** (`App.xaml.cs`)
- Single instance via a named mutex.
- Tray icon with Start/Pause, Reset, Skip, Settings and Exit as a backup control surface.
- When Deskband11Lib raises `TaskbarWindowRecreationRequired`, the host window is disposed and rebuilt after a short delay.

## Known limitations

1. Primary monitor's taskbar only.
2. Auto-hide taskbar isn't handled.
3. Overlay mode uses a fixed offset from the right edge of the screen.
4. The embedding depends on undocumented Explorer internals and can break with Windows updates.
