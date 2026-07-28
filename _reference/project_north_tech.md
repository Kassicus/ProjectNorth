# PROJECT NORTH — Technical Design Document
**Version 0.2 — Working Draft** · Companion to project_north_gdd.md (§11) and project_north_lore.md

> **v0.2 changelog:** Logistics resolved — solo developer (code + art), own pixel art from the start, PC/Steam only. §8 updated with implications; §5 pixel-density recommendation firmed up for solo art throughput.
> **v0.3 changelog:** Repo layout revised (§2): Sim and tests live at repo root beside `game/`, and the root solution excludes the Godot project so `dotnet test` never needs the Godot SDK (CI-friendly). Handoff documents created for Claude Code: `CLAUDE.md` draft + M0 implementation brief.

> **[CORE — decided]:** Godot 4.x with .NET (C#). 2D top-down pixel art in the Stardew Valley lineage, slightly higher pixel density. **Dynamic, spatial weather**: weather systems roll across the map rather than being a per-day flag; storms are genuine travel hazards that can injure (or kill, on higher difficulties).

---

## 1. Stack

- **Engine:** Godot 4.x (.NET build). Pin to the current stable 4.x at repo creation; upgrade deliberately at milestone boundaries, never mid-milestone.
- **Language/runtime:** C# on the .NET SDK version Godot's release targets. Enable nullable reference types and treat warnings as errors from day one.
- **IDE:** whatever you like — Rider and VS Code (with the C# Dev Kit + Godot support) are both solid with Godot Mono.
- **Version control:** Git. Godot 4 scene/resource files are text-based (.tscn/.tres) and diff acceptably; add the standard Godot .gitignore (.godot/ folder especially). Git LFS for art/audio source files (Aseprite files, PSDs, WAVs) — exported PNGs/OGGs are usually fine in plain Git.
- **[OPEN]** CI (build + headless test runner via GitHub Actions) — cheap to add early, recommended by M1.

## 2. Repository & Project Structure **[PROPOSAL]**

```
project-north/
  CLAUDE.md                 # standing instructions for Claude Code sessions
  ProjectNorth.sln          # Sim + Tests ONLY (Godot manages its own project in game/)
  docs/                     # these design documents — source of truth
  src/
    ProjectNorth.Sim/       # PURE C# simulation — no Godot types allowed
      Calendar/             #   GameClock, GameDate, SeasonController
      Core/                 #   SimRng, SimVec2
      Weather/              #   IWeatherProvider, WeatherSample; M1: ClimateDirector
      Needs/  Temperature/  Economy/  Crafting/  World/  Events/   # added per milestone
      Save/                 #   SimState, versioned SaveSystem
  tests/
    ProjectNorth.Sim.Tests/ # xUnit suite against Sim (headless, runs in CI)
  game/                     # the Godot 4.x project
    project.godot
    ProjectNorth.Game.csproj  # references ../src/ProjectNorth.Sim
    scenes/  assets/
    data/                   # authored content: events/ recipes/ items/ manifests/ dialogue/
    src/
      Bridge/               # thin adapters: Sim state <-> Godot nodes/signals
      Presentation/         # Godot-facing C#: controllers, UI, VFX drivers
  tools/                    # balance harness, content validators, save inspectors
```

Why the root solution excludes the Godot project: resolving `Godot.NET.Sdk` requires NuGet + version alignment with the installed editor, and CI has neither. Keeping `dotnet build`/`dotnet test` pure-.NET at the root means the test suite runs anywhere; the Godot editor builds `game/` on its own.

**The load-bearing rule: `Sim/` never references Godot.** All simulation (weather, needs, temperature, economy, calendar, events) is pure C#, tick-based, deterministic, and seeded. Consequences:
- The entire game logic runs **headless** — unit-testable, and you can write a console harness that simulates 10 in-game years overnight to catch balance drift (does a prepared player actually survive Act 4? run it).
- Determinism makes the weather **forecastable** (§4) and makes save/replay bugs tractable.
- Presentation reads Sim state and renders it; input produces Sim commands. One direction each way.

## 3. Core Architecture **[PROPOSAL — firming up GDD §11]**

- **GameClock:** authoritative tick source. In-game minutes per real second as a tunable; everything subscribes to ticks/day-rollover/week-rollover (plane) /season-rollover. **Season is a scriptable state machine, not a function of the date** — `SeasonController` normally advances Spring→Summer→Autumn→Winter, but story phases can pin it (the Winter That Stays) or *reverse* it (the False Thaw is literally a scripted state transition Spring→Winter — build this in from the first commit).
- **Event layer:** data-driven definitions (id, trigger conditions: date window, location, attunement, weather, flags; payload: dialogue, spawn, flag set, manifest change). A single evaluator runs on relevant ticks. Anomalies, plane beats, and news items are all just events — writers add .tres/JSON files, no recompiles.
- **World state & flags:** one serializable `WorldState` aggregate — the save file is essentially this plus Sim service states. Versioned envelope (`saveVersion`, migration functions) from the very first save you ever write, because retrofitting save migration is misery.
- **Difficulty (GDD §5.2):** a `DifficultyProfile` resource — consumption rates, Class F caps, storm lethality, death mode — injected into Sim services at new game. Tiers are just data.

## 4. The Weather System **[CORE concept — PROPOSAL implementation]**

### 4.1 Design intent
Weather is **spatial and moving**, not a daily coin-flip. A storm is *somewhere*, heading somewhere, and the player can be caught by it. Effects:
- Planning gameplay: any trip beyond the homestead is a decision made against the sky. Reading conditions before hiking north is a learnable, masterable skill.
- Danger gameplay: exposure in a storm drains warmth fast, can injure (frostbite, hypothermia stages), and on higher difficulty tiers can kill (per `DifficultyProfile`).
- Narrative gameplay: in Act 3+, the *same trusted system* starts producing wrongness — storms against the wind, cold snaps with no front, weather the barometer didn't see — so the player feels the breach in their bones, not via cutscene.

### 4.2 Simulation model **[PROPOSAL]**
Front-based, entity-driven (cheap, legible, scriptable — preferred over a full fluid/cell simulation, which is overkill at our map scale):

- A **ClimateDirector** (per season, per story phase) spawns **WeatherSystem** entities off-map with: type (rain front, snow squall, whiteout blizzard, cold snap, clear high-pressure ridge), position, velocity vector (prevailing NW→SE with variance), radius/shape, intensity curve over lifetime, and a pressure signature.
- **Local weather at any point** = base seasonal climate blended with all overlapping systems: precipitation, wind speed & direction, temperature offset, visibility. Exposed through one API: `WeatherSample GetWeather(Vector2 worldPos, GameTime t)`.
- Because the Sim is deterministic and seeded, `GetWeather` can be queried **for future t** — which is exactly what forecasting instruments do (below). Scripted story weather (the False Thaw, the endless winter, anomalous events) works by the ClimateDirector switching spawn tables and by hand-authored WeatherSystem entities from the event layer.
- Temperature coupling: `EffectiveTemp = ambient(season, timeOfDay, elevation?) + systemOffsets − windChill(wind) `, feeding the existing warmth/insulation/wetness model. Wetness from rain/snowmelt multiplies heat loss — getting soaked far from shelter *is* the emergency.

### 4.3 Player-facing weather craft **[PROPOSAL]**
Forecasting is diegetic, skill-based, and improves with investment — never a UI widget that just tells you:
- **Sky reading:** distant systems render on the horizon before arrival (darkening cloud banks in their true direction of approach; wind picking up ahead of a front). Free, always on, rewards attention.
- **The barometer:** an orderable instrument (early, cheap, essential — the kind of thing grandfather's notes tell you to order first). Shows pressure and trend — mechanically, a sanctioned peek at `GetWeather(here, now + N hours)` expressed as falling/steady/rising.
- **Radio forecasts:** regional forecasts while civilization holds — increasingly wrong in Act 3 (the forecasters' models are breaking with the world), then gone. Their growing wrongness is itself storytelling.
- **Grandfather's weather-lore:** journal marginalia keyed to this map ("wind backing east off the lake by noon means snow by dark") — local knowledge as content.
- **The dog:** unsettled before big systems; another organic sensor.
- **Anomalous weather** deliberately breaks these tools — the barometer says rising while the sky turns black. Tool betrayal = wrongness made mechanical.

### 4.4 Exposure, injury, and shelter **[PROPOSAL]**
- **Exposure ladder:** chilled → shivering (energy drain, slower actions) → frostbite risk on extremities (injury: reduced capability until treated) → hypothermia stages (vision/control degradation) → collapse. Collapse resolves per difficulty tier: rescue-fantasy wake-up at the cabin (softcore tiers, with real cost) vs. death (Permafrost).
- **Whiteout rules:** hard visibility radius; landmarks vanish; there is no minimap to fall back on (GDD §5.3 — the paper map + compass *is* navigation), so getting lost is the mechanic. Rope lines and blaze marks (craftable) are the counterplay near home; a well-plotted map with bearings to shelter is the counterplay in the far field.
- **Shelter network:** grandfather's trapline cabins as storm refuges (stocked, if the player maintains them — a maintenance loop that makes the far map survivable); craftable emergency snow shelter/lean-to as the skill-check escape valve; fire-starting under weather pressure as a tense micro-game **[OPEN — how deep?]**.
- **Structures take weather:** wind/snow-load damage to roofs and the greenhouse after major storms → a repair loop that keeps the homestead feeling alive under siege in Act 4.

### 4.5 Rendering the weather **[PROPOSAL]**
- Presentation samples `GetWeather` around the camera and drives: GPUParticles2D (rain/snow with wind vector), a full-screen shader stack (visibility/fog, wetness darkening, screen-edge frost as exposure worsens), CanvasModulate for light level, and the audio bed (wind layers keyed to wind speed).
- Distant-system visualization (the horizon read in §4.3) via parallax cloud/sky layers driven by actual off-screen system positions — the art *is* the forecast.

## 5. Rendering & Art Pipeline **[PROPOSAL]**

- **Pixel density [OPEN — decide before any art is made; recommendation firmed for solo]:** Stardew is 16×16 tiles / ~16×32 characters. Given solo art (§8), the recommendation is now **"dense-16": 16×16 tiles with higher-detail characters, props, and lighting** — Stardew's own trick for reading as richer than its tile grid. True 32×32 tiles at 640×360 would look gorgeous but roughly 2–4× the drawing effort *per asset* across thousands of assets; on a solo project, that cost lands entirely on the critical path. Still worth the planned test: draw the cabin dooryard once at dense-16 and once at 32px, put them side by side, and decide with eyes open. Density is also partly a *palette and lighting* problem — the colder, moodier grade plus first-class lighting will do a lot of the perceived-density work for free.
- **Godot mapping:** TileMapLayers for terrain/paths/water (terrain autotiling for shorelines and snow edges — note: **snow coverage as tile-set variants** switched by Sim state, so the world visually freezes over time in Act 3); Y-sorted scene layer for entities/props; Light2D + CanvasModulate day/night; seasonal palette handled via shader LUT swap rather than re-authoring tiles **[OPEN — verify LUT approach fits the art style]**.
- **Aseprite** as the assumed art tool; export automation script in `tools/` once there's a real asset flow.

## 6. Testing Strategy **[PROPOSAL]**

- Unit tests over `Sim/` (xUnit or NUnit): calendar transitions incl. the False Thaw reversal, weather sampling determinism (same seed ⇒ same storm), needs math, economy phase transitions, save round-trip on every `WorldState` change.
- **The long-run harness:** console app that runs scripted player policies ("competent forager", "hoarder", "unprepared") through full multi-year sims and reports survival margins per difficulty — the balance instrument for Act 4 survivability (GDD §5.1) and storm lethality tuning.
- Godot-side testing kept thin (scene smoke tests); the payoff of the Sim/Presentation split is that most bugs live where tests are cheap.

## 7. Milestone Roadmap **[PROPOSAL]**

- **M0 — Walking Skeleton (small):** repo, project boots, character walks a test map, GameClock runs, one save/load round-trip, one unit test in CI. *Proves the pipeline.*
- **M1 — Proof of Concept (the GDD slice, weather-upgraded):** one map region; day/night + temperature + warmth/hunger; chop/carry/one cabin repair; **one weather front that rolls through and forces the player indoors**; one plane visit with a working order sheet; one scripted deniable anomaly. *Proves the game.* Success test: does the storm arriving while you're out feel like an event?
- **M2 — Vertical Slice:** one full season (Spring Y1) with all core loops present in rough form: farming plot, fishing, hunting, crafting tier 1, barter introduction, dog adoption, 4 plane visits, 2–3 anomaly seeds, exposure/injury v1.
- **M3 — Year One:** all four seasons, freeze-up beat, floats→skis, Act 2 anomaly ladder, difficulty tiers wired.
- **M4 — The Break:** False Thaw, Act 3 systems (manifest decay, cash death, irregular plane), observances v1.
- **M5 — The Silence:** Act 4, endgame content, endings.
- Rule of thumb: nothing enters M(n+1) while an M(n) core loop still feels bad. The clock/weather/save foundations are deliberately front-loaded because everything downstream leans on them.

## 8. Project Logistics **[Resolved v0.2 — implications & remaining opens]**

**Decided:** Solo developer, everything (code, art, design). Own pixel art from M0 onward. **PC/Steam only.**

### Implications **[PROPOSAL — read honestly]**
- **Scope reality check.** As specced — deep crafting, ~2 authored in-game years, ~28–30 authored plane visits, dynamic weather, a lore mainline, and solo art — this is a **multi-year solo project**. That's fine (Stardew was one person and ~4.5 years) but it should be a *chosen* fact, not a discovered one. The design already has the right shape for it: one authored map (no procgen art explosion), sparse audio, diegetic UI (paper props are cheap to draw), one recurring NPC portrait set, and heavy systemic reuse.
- **Art is the critical path**, not code. Every scope decision should be evaluated first as "how many unique sprites does this cost?" Dense-16 (§5) is the single biggest lever. Seasonal palette/LUT swaps and tile-variant snow coverage stretch every tile four-plus ways. Consider building the art style *around* what one person can sustain: strong palette, strong lighting, restrained animation frame counts.
- **PC/Steam only simplifies real things now:** mouse-and-keyboard-first UI (the paper order sheet and journal genuinely want a mouse), no console cert constraints, no input-abstraction tax. Controller/Deck support can be a post-1.0 question; just avoid *hostile* choices (pixel-hunting, hover-only information).
- **Solo process guardrails:** CI from M0 (the test suite is your only code reviewer); the long-run balance harness (§6) matters *more* solo — it's your QA department; keep a dev journal (fitting, for this game); timebox art experiments; and treat the M-gates in §7 as real gates — solo projects die of M3 features built on an M1 loop that never got fun.
- **Steam-specific, later:** page + wishlists typically go up around a strong vertical slice (M2-ish); Steam input, cloud saves, and achievements are cheap if the save/flag architecture (§3) is respected. Not now-problems.

### Remaining open
1. **Cadence & horizon:** hours/week available → honest M1 target date. (The roadmap is shaped by this more than by anything technical.)
2. **Pixel density spec (§5):** run the dense-16 vs. 32px side-by-side test; commit before mass-producing tiles.
3. **Audio plan:** own SFX from libraries + minimal music? Licensed sparse score? (The sparse-audio design keeps this small either way; decide by M2.)
