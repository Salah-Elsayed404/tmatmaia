# Tmatmaia — Design

A Pomodoro timer for Windows 11 that sits inside the empty area of the taskbar.

## Decisions

| Topic | Decision | Why |
|---|---|---|
| Stack | C# / .NET 10 / WPF | Native Windows UI. Deskband11Lib.Wpf targets `net10.0-windows10.0.26100.0`, so the app does too. |
| Taskbar placement | [Deskband11Lib](https://github.com/airtaxi/Deskband11Lib) reparents the window into `Shell_TrayWnd` and uses UI Automation to find free space | Windows 11 removed DeskBands. A real child of the taskbar is never covered when the taskbar is clicked, unlike a topmost overlay. The library handles Explorer restarts, DPI changes and secondary monitors. |
| No overlay fallback | Removed in favor of a single embedded mode | An earlier `Overlay` option (topmost window over the taskbar) cluttered Settings and was rarely needed. |
| End-of-phase sound | Built-in `%WINDIR%\Media` alarms/rings or any `.wav`, played with `SoundPlayer` | No extra dependencies; WAV is what `SoundPlayer` supports. A missing file falls back to the system exclamation sound. |
| Base project | Built from scratch | The closest existing project, taskbar-focus (Rust), is a tray icon plus an optional floating window, not a taskbar widget. |
| Settings storage | JSON in `%APPDATA%\Tmatmaia\settings.json` | Simple and easy to back up. |
| Icon | Drawn from code by `scripts/make-icon.ps1` | Anyone can tweak and regenerate it without design tools. |

## Behavior

**Taskbar widget** (270×48)
- Large `mm:ss` time with the phase (`Focus #n`, `Break`, `Long break`) beside it, and a thin progress bar (red for focus, green for breaks).
- One rounded frame holds the timer, the phase and the ▶/⏸ start/pause, ⟲ reset and ⚙ settings buttons (Segoe Fluent Icons glyphs), with the progress bar along its bottom edge.
- The timer text itself is not clickable; all actions go through the buttons.

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
3. The embedding depends on undocumented Explorer internals and can break with Windows updates.
