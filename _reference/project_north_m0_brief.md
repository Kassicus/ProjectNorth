# PROJECT NORTH — M0 Implementation Brief (Walking Skeleton)
**For execution in the project repo with Claude Code.** Companion to `docs/project_north_tech.md` §7 and `CLAUDE.md`.

## Goal

Prove the pipeline end to end: a repo where `dotnet test` passes on a pure-C# simulation, and a Godot project boots, drives the sim clock, and displays the date — with the calendar/season split and versioned saves in place from the very first commit. No gameplay yet. M0 is done when the Definition of Done at the bottom is fully checked.

## Step 0 — Environment & repo

1. Confirm installed versions: Godot 4.x (.NET build) and the .NET 8 SDK. Record the exact Godot minor version — it dictates the `Godot.NET.Sdk/x.y.z` version in the game csproj and the `config/features` string in `project.godot`.
2. Initialize Git. `.gitignore` covers: `game/.godot/`, `bin/`, `obj/`, `*.user`, IDE folders, OS junk. Use Git LFS for art/audio *source* files (`.aseprite`, `.wav`) when they appear — not needed at M0.
3. Create the layout from `CLAUDE.md` (docs/, src/, tests/, game/, tools/). Copy the three design docs and `CLAUDE.md` into place first — they are the standing context for every future session.
4. Root `ProjectNorth.sln` contains **Sim and Tests only**. The Godot project manages its own build inside `game/`; keeping it out of the root sln means `dotnet build`/`dotnet test` never need the Godot SDK resolvable (important for CI).

## Step 1 — `src/ProjectNorth.Sim` (pure C# class library)

Project settings: `net8.0`, `Nullable` enabled, `TreatWarningsAsErrors`, `ImplicitUsings`, root namespace `ProjectNorth.Sim`. **No package references. No Godot references — ever** (CLAUDE.md rule 1).

Build these, in this order:

### 1a. `Core/SimVec2.cs`
Engine-free `readonly record struct SimVec2(float X, float Y)` with `Zero`, `Length`, `LengthSquared`, and `+ - *(scalar)` operators. Exists solely so Sim never needs `Godot.Vector2`.

### 1b. `Core/SimRng.cs`
Deterministic seeded RNG. Contract:
- `SimRng(ulong seed)` — scramble the seed through splitmix64 so sequential seeds (1, 2, 3…) still produce well-distributed streams; map seed 0 to a nonzero constant (xorshift state must never be zero).
- Core generator: xorshift64* (or PCG — implementer's choice; keep it ~20 lines and dependency-free).
- `ulong NextUInt64()`, `double NextDouble()` in [0,1), `int NextInt(int minInclusive, int maxExclusive)` with bounds validation, `float NextFloat(float min, float max)`.
- `SimRng Fork(ulong streamId)` — derives an independent deterministic child stream **without advancing the parent** (hash parent state with streamId; do not mutate). One child stream per system (weather, wildlife, loot) is the intended usage pattern.
- `ulong State { get; }` and `static SimRng FromState(ulong)` for save/restore.

### 1c. `Calendar/Season.cs` + `Calendar/GameDate.cs`
- `enum Season { Spring, Summer, Autumn, Winter }`.
- `readonly record struct GameDate(int Year, Season CalendarSeason, int DayOfSeason)` with constants `DaysPerSeason = 28`, `SeasonsPerYear = 4`, `DaysPerYear = 112`, `DaysPerWeek = 7` (GDD §3, decided v0.3).
- `static GameDate FromTotalDays(int totalDays)` where day 0 = Year 1, Spring, Day 1; `int TotalDays` inverse; `int WeekOfSeason` (1–4); `bool IsWeekBoundary` (day 7/14/21/28 — the plane's cadence); readable `ToString()` (`"Y1 Spring 12"`).
- **Doc-comment the load-bearing idea:** `CalendarSeason` is what the wall calendar *says*; the season actually outside belongs to `SeasonController` and may disagree. Never derive gameplay season from the date (CLAUDE.md rule 3).

### 1d. `Calendar/GameClock.cs`
Authoritative tick source. Time in **sim minutes** (`MinutesPerDay = 1440`); Presentation decides sim-minutes-per-real-second.
- State: `long TotalMinutes` (ctor-restorable); derived `MinuteOfDay`, `TotalDays`, `GameDate CurrentDate`.
- `void Advance(int minutes)` — walks forward day-by-day internally so that **one large Advance fires every boundary event it crosses, once each, in chronological order**. Per completed day, fire in fixed order: `DayEnded(dateOfEndedDay)` → `WeekEnded(dateOfEndedDay)` if week boundary → `CalendarSeasonChanged(newDate)` if the calendar slot changed → `YearEnded(newDate)` if the year changed.
- Events as `event Action<GameDate>?`. Negative arguments throw. No wall-clock time anywhere.

### 1e. `Calendar/SeasonController.cs`
Owns the season that is *actually outside*.
- `enum SeasonMode { FollowCalendar, Pinned }`.
- Ctor takes the `GameClock`, subscribes to `CalendarSeasonChanged`; in `FollowCalendar` mode the actual season tracks the calendar; in `Pinned` mode calendar rollovers are ignored (the calendar keeps flipping pages; the world does not follow).
- API: `Pin(Season)` (→ Pinned; e.g. `Pin(Winter)` = the Winter That Stays), `ForceSeason(Season)` (set actual without changing mode — scripted sequences), `Release()` (→ FollowCalendar, snap to calendar), `RestoreState(mode, season)` (save-load path, fires no events).
- `event Action<Season, Season>? SeasonChanged` (old, new) — fired only on real changes.
- The **False Thaw** must be expressible as: `Pin(Spring)` … days pass … `Pin(Winter)` — and a test proves it (Step 2).

### 1f. `Weather/WeatherSample.cs` (API stub only — real model is M1)
- `readonly record struct WeatherSample(float TemperatureC, float WindSpeedKph, float WindDirectionDeg, float PrecipitationIntensity, float Visibility01)` with one named preset (e.g. `MildSpringDay`).
- `interface IWeatherProvider { WeatherSample GetWeather(SimVec2 worldPos, long timeMinutes); }` — doc-comment that determinism makes future-time queries valid (this is the barometer, TECH §4.2/§4.3).
- `ConstantWeatherProvider` returning a fixed sample — the M0 placeholder the ClimateDirector replaces in M1.

### 1g. `Save/` + `SimWorld.cs`
- `SimState` — serializable record snapshot: `Version`, `Seed`, `RngState`, `TotalMinutes`, `SeasonMode`, `ActualSeason`. Every future Sim system adds its state here.
- `SaveSystem` — `CurrentVersion = 1`; `Serialize(SimState)` / `Deserialize(string)` via `System.Text.Json` (enum-as-string converter, indented). Deserialize reads `Version` first: **newer than supported → throw a dedicated `SaveVersionException`; older → explicit migration chain** (a switch that currently only handles v1, structured so v1→v2 slots in later). Never silently reinterpret (CLAUDE.md rule 4).
- `SimWorld` — facade owning `Seed`, `Rng`, `Clock`, `Seasons`, `Weather`. Public ctor `SimWorld(ulong seed)`; `SimState CaptureState()`; `static SimWorld Restore(SimState)`. The Bridge talks only to `SimWorld` — Presentation never constructs Sim services directly.

## Step 2 — `tests/ProjectNorth.Sim.Tests` (xUnit)

Packages: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` (current stable versions). Minimum suite — these encode design decisions, not just correctness:

**GameDate:** day 0 = Y1 Spring 1 · day 27 = Y1 Spring 28 and day 28 = Y1 Summer 1 · day 112 = Y2 Spring 1 · `TotalDays` round-trips across ≥3 years.
**GameClock:** one day's Advance fires `DayEnded` exactly once and lands on `MinuteOfDay == 0` · 7 days fires `WeekEnded` once · a single 28-day Advance fires 28 DayEnded, 4 WeekEnded, 1 CalendarSeasonChanged and lands in Summer · `Advance(1439)` fires nothing, the next `Advance(1)` fires the rollover.
**SeasonController:** follows calendar by default · **the Winter That Stays**: after Y1, `Pin(Winter)`, advance two more season slots → calendar says Summer, `ActualSeason` is Winter · **the False Thaw**: enter Y2 Spring, `Pin(Spring)`, advance ~10 days, `Pin(Winter)` → actual is Winter, the (Spring→Winter) transition was observed via `SeasonChanged`, and the calendar still reads Spring · `Release()` snaps back to the calendar.
**SimRng:** same seed ⇒ identical sequence · different seeds diverge · `Fork` is deterministic and does not advance the parent (compare `State` before/after) · `NextInt` respects bounds over many draws.
**Saves:** capture → serialize → deserialize → restore preserves clock, date, mode, and season · **a restored world continues in lockstep with the original** (identical subsequent RNG draws; identical dates after identical Advances) · a tampered `Version: 999` save throws `SaveVersionException`.

Gate: `dotnet test` green from the repo root.

## Step 3 — `game/` (Godot project)

1. Create the Godot project in `game/` via the editor (correct 4.x scaffolding beats hand-written config). Then adjust:
   - `ProjectNorth.Game.csproj`: `Godot.NET.Sdk` version matching the installed editor; `net8.0`; `Nullable`; add `<ProjectReference Include="..\src\ProjectNorth.Sim\ProjectNorth.Sim.csproj" />`.
   - `project.godot`: name "Project North"; main scene `res://scenes/Main.tscn`; **pixel-art display settings** — viewport 640×360, window override 1280×720, stretch mode `viewport`, default texture filter Nearest.
2. `scenes/Main.tscn`: a `Node2D` root with the `Main.cs` script and a `Label` child named `ClockLabel`.
3. `game/src/Presentation/Main.cs` — the bridge proof, and the *only* sanctioned pattern (CLAUDE.md rule 6):
   - Exported `float SimMinutesPerRealSecond = 60f`.
   - `_Ready`: construct `new SimWorld(seed)`, cache the label, subscribe `WeekEnded` → `GD.Print("[plane day] ...")` and `SeasonChanged` → `GD.Print("[season] from -> to")`.
   - `_Process`: accumulate `delta * SimMinutesPerRealSecond` into a double, `Advance` only whole minutes, keep the fractional remainder (no drift).
   - Label shows: `Y1 Spring 1  06:00  (outside: Spring)` style — date, time, and **actual** season, so the calendar/actual split is visible on screen from day one.

Manual check: run the scene at high `SimMinutesPerRealSecond`; watch days tick, `[plane day]` log on day 7, season flip at day 28.

## Step 4 — CI

GitHub Actions workflow: on push/PR → setup .NET 8 → `dotnet test ProjectNorth.sln`. Sim-only (no Godot in CI at M0 — this is why the root sln excludes the game project). The test suite is the solo developer's only code reviewer; CI makes it unskippable.

## Definition of Done (M0)

- [ ] Repo initialized; layout matches `CLAUDE.md`; design docs in `docs/`
- [ ] `dotnet test` green: GameDate, GameClock, SeasonController (incl. Winter-That-Stays and False-Thaw tests), SimRng, save round-trip + version rejection
- [ ] Godot project boots; clock label advances; `[plane day]` and season logs appear at the right boundaries
- [ ] `SimWorld` save/restore proven by test to continue in lockstep
- [ ] CI running the test suite
- [ ] Zero Godot references anywhere under `src/ProjectNorth.Sim/` (grep for `Godot` as a final check)

## Explicitly NOT in M0

Player character/movement, tilemaps, art, input map, the real weather model (M1), needs, the order sheet, any content. Resist. The skeleton must walk before it hunts.

## First prompts to try in Claude Code

1. "Read CLAUDE.md and docs/, then execute Step 0 and Step 1 of docs/m0_brief.md. Stop before tests."
2. "Write the Step 2 test suite. Run it and fix failures — tests encode the brief's contracts; if code and brief disagree, the brief wins."
3. "Set up the Godot project per Step 3. My Godot version is ⟨X.Y⟩."
4. "Add the CI workflow from Step 4 and confirm a clean run."
