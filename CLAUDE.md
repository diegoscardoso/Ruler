# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Ruler is a Windows desktop utility for visually measuring pixel distances on screen. The user draws free-direction lines over the desktop (click-drag-release), which stay on top of every application and show their length in pixels. Built with .NET 8 WinForms plus native Win32 layered-window APIs, no external dependencies.

## Build & run

```
dotnet build Ruler.sln -c Debug
dotnet run --project Ruler\Ruler.csproj
```

There are no tests and no CI configuration in this repo.

## Architecture

The solution (`Ruler.sln`) contains a single project: `Ruler\Ruler.csproj` (`net8.0-windows`, WinForms, `StartupObject` = `Ruler.Program`).

- **`Program.cs`** — entry point. Sets `HighDpiMode.PerMonitorV2` (so all coordinates are physical pixels — the ruler measures real pixels) and runs a single `RulerOverlay`.
- **`RulerOverlay.cs`** — the entire app. A borderless, `TopMost` form covering the whole virtual screen (all monitors), created with `WS_EX_LAYERED` and rendered via `UpdateLayeredWindow` (see `NativeMethods.cs`). Every frame is drawn with GDI+ into a 32bpp ARGB bitmap and pushed to the window; there is no WM_PAINT path and no child controls.
  - **Per-pixel hit-testing is the core trick**: pixels with alpha 0 are click-through (mouse goes to the app below); pixels with alpha ≥ 1 receive mouse input. `HitOnlyColor` (alpha 1, visually imperceptible) is used to (a) fill the whole background while drawing is armed, so a drag can start anywhere, and (b) widen the clickable stroke/handles around the thin visible lines.
  - Holds a `List<MeasureLine>` (Start/End/Color) plus a `selected` line, which is the only one showing endpoint handles. Drawing is "armed" (`newLineArmed` or empty list) → background captures the mouse and press-drag-release creates a line (`DragMode.Drawing`), which becomes selected. Otherwise the background is alpha 0 and only lines intercept clicks: clicking selects the nearest line (`FindNearestLine`) and drags it (`MoveLine`); the selected line's endpoint handles resize it (`MoveStart`/`MoveEnd`).
  - Keys (require focus — any click on the overlay/lines calls `Activate()`): `N` arms drawing of an additional line; `Esc` cancels an armed `N`, else deletes the selected line, else (no lines left) exits the app; `1`/`2` set the selected line's color (LimeGreen/Red) and the default for new lines.
  - A length label ("N px") is drawn beside each line's midpoint.
- **`NativeMethods.cs`** — P/Invoke for `UpdateLayeredWindow`, `GetDC`, `CreateCompatibleDC`, `SelectObject`, etc. `SetLayeredWindowBitmap(form, bitmap)` pushes the ARGB bitmap with `AC_SRC_ALPHA` blending.

Nothing is persisted across sessions (the old `Properties.Settings` mechanism was removed along with the previous thin-form implementation).

### Known dead/unfinished code — do not assume this is wired up

- **`RulerWPF/`** — a separate WPF project sitting alongside `Ruler/` in the repo, but **not included in `Ruler.sln`**. It's just the default WPF template (`MainWindow.xaml` with one tweaked `Grid` opacity) — no real logic. Treat as an abandoned/parked experiment unless explicitly asked to revive it.
