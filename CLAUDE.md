# CLAUDE.md — Project North

## What this project is

Project North is a 2D top-down survival life-sim with a supernatural slow-burn mystery — a cozy homestead game that freezes into an arcane survival story. Solo developer. Godot 4.x (.NET/C#), pixel art, PC/Steam only.

**The design documents in `docs/` are the source of truth:**
- `project_north_gdd.md` — game design: pillars, acts, systems, open questions (§12 is the live decision agenda)
- `project_north_lore.md` — cosmology and narrative truth (design-side; the player learns it in fragments)
- `project_north_tech.md` — architecture, weather system, milestone roadmap (M0–M5)

When code and docs disagree, flag it — don't silently pick one. When a design question isn't answered in the docs, ask; don't invent a decision and bury it in code. Small implementation choices are fine to make; anything marked **[OPEN]** in the docs is not yours to resolve.

## Pinned toolchain

| Thing | Version | Where it's pinned |
|---|---|---|
| Godot editor | **4.6.2.stable.mono** (`/Applications/Godot_mono.app`) | `game/project.godot` → `config/features` |
| `Godot.NET.Sdk` | **4.6.2** — must match the editor minor | `game/ProjectNorth.Game.csproj` |
| .NET SDK | **9.0.x** | `global.json` (`rollForward: latestFeature`) |
| Target framework | **`net9.0`** everywhere | `Directory.Build.props` |

⚠️ **`net9.0`, not the `net8.0` written in the M0 brief.** Godot 4.6.2 generates `net9.0`
csprojs, and `game/` holds a `ProjectReference` to `src/ProjectNorth.Sim`, so Sim can never
target a framework *newer* than the game project. Signed off 2026-07-28. Don't "fix" this
back to net8.0 — the brief predates the installed toolchain.

There is also a non-mono **Godot 4.4.1** at `/Applications/Godot.app`. It cannot run C# at
all. Always open this project with `Godot_mono.app`.

Upgrade Godot only at milestone boundaries (TECH §1), and bump editor + SDK + `config/features` together.

## Architecture rules (non-negotiable)

1. **`src/ProjectNorth.Sim/` never references Godot.** No Godot types, no Godot usings, no engine assumptions. It is a pure C# class library: tick-based, deterministic, headless-runnable. The Bridge/Presentation layer (inside `game/`) converts at the boundary (`SimVec2` ⇄ `Vector2`, etc.).
2. **Determinism is sacred.** All randomness flows through seeded `SimRng` instances (never `System.Random`, never `Guid.NewGuid()`, never wall-clock time inside Sim). A seed must fully determine a run — this is what makes weather forecastable (the barometer queries future sim state) and the balance harness reproducible.
3. **Calendar ≠ season.** `GameDate.CalendarSeason` is what the wall calendar says; `SeasonController.ActualSeason` is what's outside. They diverge by script (the False Thaw, the Winter That Stays). Never derive gameplay season from the date.
4. **Saves are versioned from the first write.** Every `SimState` shape change bumps the save version and adds an explicit migration. Never silently reinterpret old saves; reject newer-than-supported versions loudly.
5. **Data-driven content.** Story beats, anomalies, plane manifests, recipes, and dialogue live in data files under `game/data/`, evaluated by the event layer — not hardcoded in C#.
6. **One direction each way.** Presentation reads Sim state and issues Sim commands. Sim raises events/exposes state. No Presentation logic reaching into Sim internals, no Sim callbacks that assume a renderer exists.

## Working conventions

- **Tests accompany Sim changes.** Every Sim behavior change lands with xUnit coverage in `tests/`. The test suite is the solo developer's only code reviewer. `dotnet test` must pass before any work is considered done.
- **C# style:** nullable enabled, warnings as errors, `LangVersion` latest. XML doc comments on public Sim APIs — especially *why* comments on anything with design intent behind it (cite the doc section, e.g. `// GDD §5.1`).
- **Naming:** in-fiction terms use the lore glossary (LORE doc §8) — `Vintermark`, `vardsten`, `Undervint` — but only once those names are signed off; use neutral names (`WardstoneSite`) for anything still **[OPEN]**.
- **Milestone discipline (TECH §7):** current milestone gates the work. Don't build M2 features while an M1 loop feels bad. If asked to jump ahead, note the gate and confirm.
- **Godot files:** let the editor own `.godot/`, import files, and UID churn; keep `project.godot` diffs minimal and deliberate. The `Godot.NET.Sdk` version in `game/ProjectNorth.Game.csproj` must match the installed Godot minor version.
- **Commits:** small, present-tense, scoped (`sim: clock fires WeekEnded on day 7`, `game: bridge clock label`). Never commit failing tests.

## Repo layout

```
ProjectNorth/
  CLAUDE.md                     # this file
  ProjectNorth.sln              # Sim + Tests (Godot manages its own project inside game/)
  Directory.Build.props         # repo-wide C# settings (net9.0, nullable, warnings-as-errors)
  global.json                   # pins the .NET SDK to 9.0.x
  docs/                         # the three design documents (source of truth) + the M0 brief
  src/ProjectNorth.Sim/         # pure C# simulation (Calendar/ Core/ Weather/ Needs/ Economy/ Save/ ...)
  tests/ProjectNorth.Sim.Tests/ # xUnit suite against Sim
  game/                         # the Godot 4.x project
    project.godot
    ProjectNorth.Game.csproj    # references ../src/ProjectNorth.Sim
    scenes/  assets/  data/
    src/Bridge/                 # Sim ⇄ Godot adapters
    src/Presentation/           # controllers, UI, VFX drivers
  tools/                        # balance harness, content validators, save inspectors
```

## Current state

- **Milestone:** M0 (walking skeleton) — see `docs/project_north_tech.md` §7 and the M0 brief.
- **Live decision agenda:** GDD §12. Highest-priority opens: lore naming sign-off, salt/mineral site, crafting spec pass, cartography depth.
