# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Ruler is a Windows desktop utility: a thin, frameless, always-on-top, draggable horizontal line used to visually measure pixel distances on screen. Built with .NET 8 WinForms, no external dependencies.

## Build & run

```
dotnet build Ruler.sln -c Debug
dotnet run --project Ruler\Ruler.csproj
```

There are no tests and no CI configuration in this repo.

## Architecture

The solution (`Ruler.sln`) contains a single project: `Ruler\Ruler.csproj` (`net8.0-windows`, WinForms, `StartupObject` = `Ruler.Program`).

- **`Program.cs`** — entry point, launches `Form1`.
- **`Form1.cs`** — the entire app. A borderless, `TopMost`, draggable form containing one `Panel` (`linePanel`) rendered as a thin colored line; the surrounding form chrome is invisible via `TransparencyKey` (khaki). All behavior lives in `Form1_KeyDown`/`Form1_MouseDown/Move/Up`/`Form1_FormClosing`:
  - Drag to move (left mouse button)
  - `Esc` closes the ruler
  - `1` / `2` set line color to LimeGreen / Red
  - `Left` / `Right` arrows shrink/grow width by `SizeChangeValue` (10px), clamped to a minimum of 1px
  - `N` opens an additional independent `Form1` instance (`new Form1().Show()`)
  - Position, width, and line color persist across sessions via `Properties.Settings.Default`, saved in `Form1_FormClosing` and restored in the constructor (`RestoreFormPosition`/`RestoreSizeWidth`/`RestorePanelColor`)
- **`Properties/Settings.settings`** — defines the persisted user settings (`FormPosX`, `FormPosY`, `LineColor`, `SizeWidth`, plus unused `VerticalFormPosX`/`VerticalLineColor`/`VerticalSizeHeight` provisioned for the vertical ruler below).

### Known dead/unfinished code — do not assume these are wired up

- **`VerticalForm.cs`** — a stub for a not-yet-implemented vertical ruler counterpart to `Form1`. The Designer file is still the bare WinForms template (no `linePanel`, no event handlers). It is never instantiated anywhere. Settings fields exist for it, but no logic does.
- **`Ruler.cs`** — a leftover empty WinForms project-template scaffold (named after the project itself). Never instantiated or referenced.
- **`RulerWPF/`** — a separate WPF project sitting alongside `Ruler/` in the repo, but **not included in `Ruler.sln`**. It's just the default WPF template (`MainWindow.xaml` with one tweaked `Grid` opacity) — no real logic. Treat as an abandoned/parked experiment unless explicitly asked to revive it.

`Form1`, `VerticalForm`, and `Ruler` have no shared base class despite the apparent intent for `VerticalForm` to mirror `Form1`.
