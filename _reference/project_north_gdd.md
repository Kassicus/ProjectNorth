# PROJECT NORTH — Game Design Document
**Version 0.6 — Working Draft**
**Engine:** Godot 4.x (Mono) / C# / .NET
**Genre:** Top-down survival life-sim with narrative mystery (Stardew Valley × The Long Dark × supernatural slow-burn)

> Legend: **[CORE]** = established/decided concept. **[PROPOSAL]** = suggestion to react to. **[OPEN]** = unresolved design question.
> Companion documents: **project_north_lore.md** (cosmology) · **project_north_tech.md** (engine, architecture, weather system, roadmap).

> **v0.6 changelog:** Player-driven cartography confirmed [CORE]: no minimap/auto-map — the player charts the world onto a generic gas-station roadmap using acquired mapping tools (compass, protractor, scale). New §5.3.
> **v0.5 changelog:** Tech stack confirmed (Godot 4.x Mono/.NET, 2D pixel art, Stardew-style but denser). **Dynamic spatial weather confirmed [CORE]** — fronts roll across the map; storms are travel hazards that can injure (or kill on higher difficulties); trips beyond the homestead require planning. Full system design in TECH doc §4. Technical design document created.
> **v0.4 changelog:** The primer problem resolved via **regression-as-mastery**: modern ammunition is Class F (finite); the player can craft a muzzleloader and black powder (Class C), technologically regressing to achieve permanent armament. Failure states resolved: **difficulty toggle spectrum** — lowest tier removes survival pressure entirely (food restores energy only), highest tier is permadeath. New §5.2.
> **v0.3 changelog:** Morale/Resolve confirmed as fourth need. Deep crafting confirmed (workbench + ordered-equipment progression: casting, milling, forging; player can eventually produce ammo, glass, metal parts). Dog confirmed — grandfather's dog is *missing* (empty dog bed as the hook); player adopts a new partner. Player↔grandfather relationship confirmed (childhood summers, then silence). Calendar confirmed: Stardew-style 28-day seasons. Added §5.1 resource-class framework for Act 4 survivability.
> **v0.2 changelog:** Lore framework decided (fictional Nordic-themed cosmology; ecological damage awakened an ancient power enacting reclamation — see LORE doc). Timeline decided (spring Y1 start, float plane, full year of seasons, false spring of Y2 reverts to winter, plane stops by calendar-winter of Y2). Economy decided (hybrid cash→barter via the pilot's home settlement).

---

## 1. High Concept **[CORE]**

Drowning in debt, the player sells everything they own and moves to their late grandfather's cabin in the remote Canadian wilderness. Armed with basic tools, a rifle, and a weekly plane resupply, they must rebuild the homestead and learn to survive. But the land holds something old. Strange things flicker at the edge of vision, escalating into undeniable arcane phenomena — until spring of the second year quietly turns back into winter. As an impossible ice age grips the northern hemisphere, the plane stops coming, and the player must survive alone while unraveling the mystery of the endless winter.

**The one-line pitch:** *A cozy homestead sim that slowly freezes into a supernatural survival mystery.*

**The emotional arc:** Relief → Competence → Unease → Dread → Isolation → Resolve.

---

## 2. Design Pillars **[PROPOSAL]**

1. **The land provides, the land takes.** Survival is fair but indifferent. Every comfort is earned. Nothing is handed to the player except what arrives on the plane — and that lifeline is finite.
2. **Slow-burn wrongness.** The supernatural must be *deniable* for as long as possible. No jump scares; escalating certainty.
3. **The plane is everything.** The weekly resupply is the game's heartbeat: economy, news, and human contact. Its eventual silence should be the most devastating moment in the game — mechanically and emotionally.
4. **Grandfather is present in absence.** The player never meets him, but the cabin, his journals, his caches, and his unfinished work make him the game's most important character.
5. **Winter is the antagonist — and it is *owed*.** The endless winter is not malice from nowhere; it is an old power enforcing broken terms (see LORE doc). The player struggles against it, then comes to understand it.

**[OPEN]** Do we want a pillar about combat/danger (wildlife, anything else), or is this fundamentally non-violent outside of hunting?

---

## 3. Narrative & Progression Arc **[CORE timeline]**

### The Calendar Spine **[CORE — decided v0.3]**
**28 days per season, 4 weeks per season, 4 seasons per year** (Stardew-style). A year is 112 days / 16 weeks / 16 plane visits. Concrete act math:

| Act | Calendar | Weeks (cumulative) | Plane |
|---|---|---|---|
| 0 — Prologue | pre-arrival | — | — |
| 1 — Foothold | Y1 Spring–Summer | wk 1–8 | weekly, floats |
| 2 — Roots & Ripples | Y1 Autumn–Winter | wk 9–16 | weekly; freeze-up gap beat ~wk 12; skis from ~wk 13 |
| 3 — False Thaw & the Winter That Stays | Y2 "Spring" onward | wk 17–~30 | irregular, decaying |
| 4 — The Silence | ~Y2 calendar-Winter onward | ~wk 29–32 start | stopped |

Scripted arc ≈ 2 in-game years (~224 days) before Act 4 goes open-ended. Roughly 28–30 total plane visits — each one authored content (news, manifest changes, dialogue), which is a tractable writing budget.

- **Year 1 Spring:** arrival (float plane onto the lake). Acts 0–1.
- **Year 1 Summer → Autumn:** foothold becomes homestead. Deniable anomalies begin. Act 1 → 2.
- **Year 1 Winter:** first true winter. Freeze-up; the pilot swaps floats for skis. Suspicious-tier anomalies. Act 2.
- **Year 2 "Spring" — the False Thaw:** the thaw *begins* — ice candles, meltwater, first buds — then stalls and reverses. Spring turns back into winter instead of summer. Act 3 begins.
- **Year 2 (calendar summer/autumn, actual winter):** permanent winter deepens; plane grows irregular; hemispheric ice-age news. Act 3.
- **Year 2 calendar-winter:** the plane stops coming. Act 4, open-ended.

**[OPEN]** Fine pacing within the frame: does Act 3 run a full year of failed seasons (wk 17–32) or compress? Exact week the plane stops. Tunable during production; frame is fixed.

### Act 0 — The City (Prologue) **[CORE + PROPOSAL]**
- Player is in debt in an unnamed Canadian city. **[CORE]**
- **[PROPOSAL]** Playable 10–15 min prologue: walk the apartment tagging belongings to sell; each sale shows a memory blurb. A small weight allowance of *kept* items comes north with you.
- Ends with the letter: the cabin is yours, and a bush pilot has been **pre-paid — years ago, by the grandfather himself — to fly you in and resupply you weekly for a year.** First breadcrumb: he knew someone would come. (Answered in LORE doc §5: the keeper-line.)

### Act 1 — Arrival & Foothold (Y1 Spring–Summer) **[CORE]**
- Float plane lands on the lake beside the cabin; this dock/beach is the game's front door. **[CORE]**
- Player arrives with tools, rifle + limited ammo, survival rations. **[CORE]**
- Core activities: repair the cabin (roof, chimney, door, insulation), water, foraging, fishing, hunting, first garden plot, basic workshop.
- Tutorialization is diegetic via grandfather's notes and first journal ("The stovepipe cracks every March. Spare sections under the floorboards.").
- Economy: player pays cash from the estate's remainder; mid-act, the pilot starts brokering **barter** with his home settlement (see §4).
- **Late-summer anomaly seeds (deniable):** a tool isn't where you left it; animals stare at the treeline; a distant figure that's a stump when approached; a half-second voice between radio stations.

### Act 2 — Roots & Ripples (Y1 Autumn–Winter) **[CORE]**
- Homestead matures: smokehouse, root cellar, greenhouse, woodshed, animal pen, trapline. The player is *competent* now — this act should feel good.
- **Freeze-up beat [PROPOSAL]:** one week the plane circles but can't land — the lake is icing but not yet solid. A missed delivery, a taste of Act 4. Next visit, the plane wears skis.
- Anomalies escalate to suspicious: auroras on the wrong nights in impossible colors; fresh carvings on old trees; tracks that stop mid-snowfield; grandfather's compass pointing somewhere other than north; snow falling upward in one hollow.
- The pilot's news turns odd: record cold snaps down south, birds migrating the wrong way, hard frost in Texas in September.
- Player discovers grandfather's **hidden second journal** — where he stopped writing about weather and started writing about the old power. Its later pages are illegible scrawl (see Attunement, §6.2).

### Act 3 — The False Thaw & The Winter That Stays (Y2) **[CORE]**
- **The False Thaw:** spring begins on schedule, then reverses over ~2 weeks. Systems the player internalized betray them: planting windows never open, ice never breaks, migratory game never returns. **[CORE]**
- The plane continues on skis (the lake conveniently — grimly — never thaws) but becomes irregular: weather, fuel shortages, the settlement's own troubles. **[CORE]**
- News darkens: an ice age settling over the planet's north; evacuations; the cash economy dying. **The week the pilot stops accepting cash** is a scripted beat — barter only, and the manifest shrinks. **[PROPOSAL]**
- Survival ratchets: greenhouse becomes the food lifeline, game is scarce and lean, firewood consumption doubles, ice fishing year-round.
- Arcane events are undeniable: wind-scoured snow exposes standing stones; lights beneath the lake ice; dreams leaving physical residue; the treeline measurably closer.
- **[PROPOSAL]** The pilot becomes the act's emotional core — flying past the point of sense out of loyalty, finally offering the player a seat out. Refusing (or missing) that last flight is the point of no return.

### Act 4 — The Silence (Y2 calendar-winter onward) **[CORE]**
- The plane stops. No announcement — a missed week, then another. **[CORE]**
- **[PROPOSAL]** The order sheet stays in the UI, uselessly; the player can keep filling out orders that will never arrive.
- Full self-sufficiency; the mystery becomes the mainline: the second journal, the wardstones, the wound in the land, and what grandfather left unfinished (LORE doc §6).

### Endings **[PROPOSAL — aligned to chosen lore; final selection OPEN]**
- **The Warden:** the player takes up grandfather's keeping. The winter recedes for the world; the player can never leave. One day, the plane returns.
- **The Restitution:** the old terms can be re-struck — but the power demands payment scaled to choices made all game (what you took from the land vs. what you observed and honored).
- **The Thaw:** full resolution; stay by choice, or fly out on the returning plane.
- **The Refusal:** ignore the mystery and simply endure. Endless winter, endless survival, quiet — a valid state for pure-homestead players.

---

## 4. Core Gameplay Loops

### Daily Loop **[PROPOSAL]**
Wake → weather & temperature check → morning chores (stove, animals, water) → main activity block → evening chores → journal/planning → sleep. Energy and hunger in the Stardew mold, plus **warmth** as a first-class need.

### Weekly Loop — The Plane **[CORE]**
- **The Order Sheet:** paper-styled UI; cargo **weight/volume limit** forces prioritization every week.
- **Economy [CORE — hybrid]:**
  - Phase 1 (Y1 spring–summer): the player spends the estate's remaining **cash** — finite and visibly dwindling.
  - Phase 2 (introduced by the pilot): **barter** with his home settlement — the player flies out furs, smoked fish, hides, birch syrup, carvings; the settlement's needs set exchange rates.
  - Phase 3 (Act 3): cash dies entirely; barter valuations shift with the collapse (ammo, fuel, seed, and medicine appreciate; luxuries crater). The changing manifest and rates tell the story of civilization's decline without a single cutscene.
- **Lead time:** orders arrive *next* visit; planning ahead is the skill. Radio emergency orders at premium cost.
- **News & mail:** each visit delivers a clipping or letter — the vehicle for the outside-world dread ramp.
- **[OPEN]** The settlement itself: name, character, and whether the player ever sees it (recommend: never — it stays voices, goods, and handwriting; pure isolation preserved).

### Seasonal Loop **[CORE]**
Full Y1 seasonal variation (planting windows, migration, freeze-up/break-up, daylight) — internalized by the player precisely so its cessation in Act 3 lands as mechanical betrayal.

### Mystery Loop **[PROPOSAL]**
Investigate anomaly → auto-sketched entry in the player journal → cross-reference grandfather's journals → unlock a lead (place, observance, object) → pursue → escalate. The journal becomes a detective board.

---

## 5. Survival & Homestead Systems **[PROPOSAL — needs dedicated spec passes]**

- **Needs [CORE]:** Hunger, Energy, Warmth, **Morale/Resolve**. Morale gives luxury goods a purpose on the order sheet and gives Act 4 teeth. Design requirement: because orderable morale items (coffee, chocolate, newspapers) are Class F finite goods (§5.1), the game must include *renewable* morale sources — the dog, the sauna, music, good meals, kept observances, milestones — or Act 4 becomes a pure death spiral.
- **Temperature model:** ambient + weather + clothing insulation + wetness + heat sources; the cabin has an insulation stat improved by repairs.
- **Weather [CORE — dynamic & spatial]:** weather systems (fronts, squalls, whiteouts, cold snaps) **roll across the map** rather than being a per-day flag. Any trip beyond the homestead is a decision made against the sky; being caught out drains warmth fast, can injure (frostbite, hypothermia stages), and can kill on higher difficulty tiers. Forecasting is diegetic and skill-based: sky reading, an orderable barometer, radio forecasts (until they fail), grandfather's weather-lore, the dog. Grandfather's trapline cabins double as storm refuges if maintained. In Act 3+, the same trusted system produces *wrongness* — storms against the wind, weather the barometer never saw — so the breach is felt mechanically. Full design: TECH doc §4.
- **Building:** repair-first, then expansion: workshop, smokehouse, root cellar, greenhouse, woodshed, sauna (morale + very appropriate to the Nordic thread), animal shelter; late-game warded/observance structures (LORE doc §6).
- **Food systems:** foraging, open-water + ice fishing, hunting (scarce ammo — every shot matters), trapping, plot → greenhouse farming, preservation (smoking, drying, root cellar — and eventually just "outside").
- **Crafting/tech tree [CORE — deep]:** workbench progression plus **ordered equipment**. The player builds out workshop stations and orders serious machinery on the plane — anvil & forge, casting setup (crucible, molds), milling machine, reloading press, glassworks — unlocking increasingly advanced production: metal parts, cast fittings, machined components, glass, ammunition. Two structural consequences: (1) the tech tree is the bridge from dependence to self-sufficiency, and (2) **the equipment only arrives while the plane still flies** — Act 2–3 becomes a quiet race the player may not realize they're running (§5.1). Full spec pass needed **[OPEN: station list, recipe tiers, power question — muscle/treadle/water/generator?]**.
- **Companion [CORE — dog confirmed]:** grandfather's dog is **missing** — the player finds a worn dog bed by the stove, a chewed leash on a hook, a food bowl. The empty bed inspires the player to adopt a partner (**[PROPOSAL]** the pilot brings a pup from the settlement in early Act 1 — possibly the game's first barter transaction). The dog warns of wildlife and *reacts to anomalies before the player can perceive them* (organic detection mechanic), and is the emotional anchor and key renewable morale source of the isolation acts. **[OPEN — lore hook]** Where is grandfather's dog? Died of age / ran off / *went somewhere* — this can be a breadcrumb (see LORE doc §10).

### 5.1 Act 4 Survivability — Resource Classes & the Countdown **[PROPOSAL — framework]**

Every consumable and durable in the game is assigned one of three classes. This taxonomy is the design tool that makes Act 4 survivable-by-design rather than by accident:

- **Class R — Renewable on-site.** Obtainable forever with labor and skill only: wood, water, game, fish, hides, tallow, forage, garden crops *with seed-saving*, birch syrup, charcoal. Available from Act 1. Act 4's floor.
- **Class C — Craftable after investment.** Producible on-site only after acquiring equipment (plane-delivered) and skills: metal parts, cast and machined components, glass, reloaded ammunition, lamp oil, tanned leather goods, remedies from an herbalism bench, tool repair/fabrication. **Class C is the game's central strategic layer:** every station the player secures before the plane stops permanently converts a dependency into independence.
- **Class F — Finite.** Cannot be produced on-site, ever: e.g. primers & powder (**[OPEN]** — see below), antibiotics/serious medicine, salt (**[OPEN]** — or Class C via a mineral spring site?), coffee/tea/chocolate/tobacco, machine spare parts beyond the player's fabrication ceiling, generator fuel. **Class F items are Act 4's countdown clocks and Act 3's stockpiling targets.**

Design consequences:
1. **The Last Orders.** As the manifest shrinks in Act 3, the player faces authored dilemmas: the milling machine or three winters of primers? Seed stock or medicine? These final order sheets should be some of the hardest decisions in the game.
2. **Telegraphing.** The game must give honest (diegetic) warning — pilot dialogue, news, price signals — so preparation is *possible* without being *prescribed*. Players who read the signs thrive in Act 4; players who didn't survive leaner, not unfairly.
3. **Difficulty knob.** Class F generosity (stockpile caps, spoilage, consumption rates) is a natural difficulty axis without touching core systems.
4. **Morale coupling.** Orderable comforts are Class F; renewable morale (dog, sauna, music, observances) must carry Act 4 (see Needs, above).

**The primer problem — resolved [CORE]: regression as mastery.** Modern rifle ammunition is **Class F**: brass can be reloaded and bullets cast, but primers and smokeless powder deplete forever, so the rifle is a dwindling asset the player rations across the whole game. The sustainable path runs *backward* through technology: with the forge, casting, and milling stations, the player can craft a **muzzleloader** (flintlock — no primers needed), and **black powder is Class C** (charcoal made on-site, saltpeter farmed from a niter bed, sulfur ordered early or sourced from a mineral site **[OPEN — tie to salt/mineral spring decision]**). The intended weapon arc: modern rifle (Acts 1–3, rationed) → bow, snares, and traps as the bridge (early Act 4) → the self-made muzzleloader as the endgame armament — a gun the player cast, milled, and stocked themselves. Regression-as-mastery is the game's thesis in a single item. **[PROPOSAL]** Grandfather began building one and never finished it — a second inheritance beside the unfinished wardstone (see §8).

**[OPEN — remaining §5.1 decisions]** (a) Salt: Class F, or Class C via a mineral spring/lick site on the map (historically *the* preservation bottleneck — bigger than it looks, and possibly the same site that supplies sulfur). (b) Full item classification pass once the item list exists.

### 5.2 Difficulty & Failure States **[CORE — difficulty toggle spectrum]**

Failure is handled by a difficulty selection at new game (not mid-game switchable upward **[OPEN — allow lowering only?]**). Proposed four tiers **[PROPOSAL — names and exact tuning open]**:

| Tier | Working name | Needs & failure |
|---|---|---|
| 1 | **Hearthside** | No survival pressure: food restores energy but is never required to live; cold slows, never kills; no death state. The homestead-and-mystery experience, pure. **[CORE: lowest tier works this way]** |
| 2 | **Settler** | Full needs simulation; failure is softcore — collapse and wake weakened at the cabin with time/resource loss. The default. |
| 3 | **The North** | Sharper consumption rates, leaner Class F stockpile caps, harsher weather events; same softcore failure. |
| 4 | **Permafrost** | Permadeath, single save. **[CORE: highest tier is permadeath]** |

Design rules: difficulty tunes *rates and consequences*, never *content* — every tier sees the full story, all anomalies, all endings. Class F generosity scales per tier (§5.1 consequence 3). Morale is active on all tiers but only threatens function (energy/speed/attunement penalties), never directly kills, on any tier **[PROPOSAL]**.

### 5.3 Cartography — The Paper Map **[CORE concept — PROPOSAL mechanics]**

**No minimap. No auto-map. The player charts the world themselves. [CORE]**

- **The starting map [CORE]:** a generic **gas-station roadmap** of the region — decades old, folded to death, showing highways, the big lakes, and a couple of town names. It's region-scale and nearly useless up close: the homestead isn't even on it. Everything the player comes to know about their world, they put there themselves.
- **Marking:** in the field the player places rough marks (icon + label) whose positional accuracy depends on method; back at the cabin **map desk [OPEN — required station or anywhere?]** marks can be refined, annotated, and inked. Recorded position ≠ true position — every mark carries a hidden confidence/error radius that better technique shrinks.
- **Mapping tools [CORE — tool list PROPOSAL]** (orderable, craftable, or findable): a **compass** (take bearings); a **protractor** (plot bearings — two bearings from known points triangulate a new one; the **fire lookout tower** is the natural first survey station); a **scale ruler** (measured distances); pacing as the free/rough distance method; better drafting supplies (finer marks, more layers). Tool progression = accuracy progression: the map visibly evolves from scrawled guesses to a surveyed document.
- **Grandfather's fragments:** pieces of his own hand-drawn maps hide in caches and the trapline cabins — accurate but partial, keyed to journal references, and annotated in his term-mark shorthand (legibility gated by attunement, like the second journal).
- **Systems interplay:**
  - **Weather/navigation (TECH §4.4):** in whiteouts the paper map + compass *is* navigation; an unplotted trapline refuge can't save you. Map quality is survival equipment.
  - **The Act 3 betrayal:** when the treeline moves and distances quietly stop agreeing, an *accurate* map becoming *wrong* is the player's own instrument testifying to the breach — the cartography version of the lying barometer.
  - **Anomaly resistance:** anomaly sites resist recording at low attunement — marks smudge, bearings won't close, the triangle never quite triangulates — and snap into place as attunement grows. The land decides what may be written down.
  - **The keeping:** plotting the wardstone boundary is literally reconstructing the contract's geography — the map is a mystery-solving surface, not just a nav aid.
- **Implementation sketch (TECH):** true world positions vs. player-recorded positions with error radii; the map UI renders only the player's record; triangulation resolves recorded position toward truth.

**[OPEN]** Simulation depth: full manual plotting (protractor as a genuine desk minigame) vs. streamlined ("use tools → mark improves") — recommend prototyping the manual version at M2 scale: if tactile drafting feels good it's a signature feature; if not, it collapses gracefully into the streamlined version. Also open: error visualization (honest circles vs. deliberately unmarked uncertainty), and whether marks can be *wrong* in ways the player must discover and correct.

---

## 6. The Magic — Presentation & Mechanics

### 6.1 Escalation Ladder **[PROPOSAL]** (roughly one tier per act)
1. **Deniable:** peripheral movement, misplaced objects, animal behavior, déjà vu framing.
2. **Suspicious:** impossible auroras, fresh carvings, interrupted tracks, the compass, radio voices, snow falling upward.
3. **Undeniable:** exposed wardstones, lights beneath lake ice, dreams leaving residue, time slips, the treeline closer.
4. **Interactive:** observances and sites the player can engage with; magic as system, not just spectacle.

### 6.2 Attunement **[PROPOSAL]**
Raised by engaging anomalies and the second journal. Higher attunement = more phenomena perceptible, more of the second journal legible (scrawl resolves into text), access to sites. **[OPEN]** Cost/trade-off: sleep quality, morale, home disturbances — or a pure key with no cost?

### 6.3 What magic does mechanically **[LEANING — confirm]**
Given the chosen lore (a contractual old power maintained through *observances*), the recommendation firms up: **rituals over spells.** The player never wields; they observe, offer, tend, and negotiate — at sites, at thresholds, on dates, under the right sky. Keeps the player human and matches the keeper-line inheritance. **[OPEN]** Confirm this, and define the observance verb set (see LORE doc §6).

---

## 7. Lore — Decided Framework **[CORE — full build-out in project_north_lore.md]**

**Decision:** man-made ecological damage exposed and awakened an ancient power that is slowly enacting its reclamation. Fully **fictional cosmology**, **Nordic-themed** (invented names and folklore in a pseudo-Scandinavian register; no real-world Indigenous cultures depicted or borrowed).

Summary of the build-out (react to details in the LORE doc):
- Winter was once a **sovereign territory with a will**, not a season — a circumpolar old power the LORE doc provisionally calls **the Vintermark**.
- Ancient peoples of the boreal ring didn't worship it; they **fenced** it — wardstones, observances, terms. Over millennia its domain shrank to deep north and deep sleep; the terms decayed into fairy tales.
- An industrial-era **wound in the land** near the cabin (provisional: an abandoned mid-century mine leaching into the lake's watershed) broke the local terms. Grandfather — descended from **stone-keepers in the old country** — recognized the signs an ocean away from where his family kept them, proving the power is circumpolar. He bought the land to watch the stones.
- His death ended the local observances. The wound + the silence = the seal failing here, while other unkept seals fail across the world's north. The hemispheric ice age is many wounds, no keepers.
- He pre-paid the plane because the keeping must pass down blood or threshold — he was betting his kin would come.

---

## 8. Characters **[PROPOSAL — names OPEN]**

- **The Player [CORE]:** customizable; defined by circumstance. Relationship to grandfather **confirmed**: *childhood summers at the cabin, then decades of silence* — the cabin carries déjà vu weight, and the keeper-line inheritance recontextualizes those summers (he wasn't just fond of you; he was *evaluating* you). Design note: seed specific childhood-summer memories in Act 1 (a carving you helped make, the swimming rock, a song) that pay off in Acts 3–4.
- **The Pilot:** the only recurring human face; gruff, kind, increasingly loyal; carries ~80% of dialogue and the economy. Needs a full character pass. **[OPEN]** Name, history with grandfather (did he suspect what the old man was?).
- **The Grandfather:** posthumous protagonist. Two journals, caches, marginalia, an unfinished work in the workshop **[OPEN: tied to LORE §6 — a replacement wardstone? a repaired observance object?]**, and a final message reachable only at high attunement. **[PROPOSAL — the two inheritances]** He leaves *two* unfinished things: the hand-cut wardstone (the keeping) and a half-built flintlock muzzleloader (the surviving) — the mystery game and the crafting game, each completed by the player, each an inheritance. He knew what was coming on both fronts.
- **Radio voices [PROPOSAL]:** a repaired shortwave; distant stations and far-north holdouts that go quiet one by one through Act 4. Late hook: one station keeps broadcasting that shouldn't.
- **The Settlement (offscreen character) [PROPOSAL]:** the pilot's home community, met only through goods, barter notes, and secondhand stories — and mourned the same way.

---

## 9. World & Setting **[PROPOSAL]**

- **Region:** fictional lake-and-boreal-forest region. **[OPEN]** Vibe: shield country (low, rocky, lake-riddled — northern Ontario) vs. mountain valley (Yukon/NWT). Shield country favors the lake-centric design; mountains favor vistas and the standing-stones ridge. Could hybridize: big lake, one hard ridge.
- **Map structure:** authored, hand-crafted map (no procgen); homestead central; concentric practical zones (dooryard → woodlot → lake → far forest) with wardstones, caches, and the wound placed to pull exploration outward. The player should learn this place like a real home — which is what makes the treeline moving so upsetting.
- **Key locations:** cabin & outbuildings; the lake (fishing, plane landings, the lights beneath); the old portage trail; a fire lookout tower; grandfather's trapline cabins (mini-bases + journal caches); the wardstone ridge; **the wound** (provisional: the abandoned mine); **[OPEN]** the final site.

---

## 10. Art, Audio & Tone **[PROPOSAL — light notes]**

- **Visual:** top-down 2D pixel art, Stardew lineage but **a touch more pixel-dense [CORE — exact spec OPEN, see TECH §5]**; colder palette; lighting as a first-class system (long dusks, aurora, firelight through windows). Weather is a visual pillar: distant fronts visible on the horizon *are* the forecast. Anomalies live in the lighting/animation layers before they ever live in sprites.
- **Audio:** sparse — wind, stove, snow crunch, loons in summer (their absence in Y2 should be *felt*). Music rare and earned. Anomalies get silence, not stingers.
- **UI:** diegetic where possible — paper order sheets, the physical journal, a wall calendar flipping pages into a winter that won't end.

---

## 11. Technical Direction **[CORE — moved to project_north_tech.md]**

Engine and stack confirmed: **Godot 4.x (.NET/C#)**. Full technical design — repository structure, the Sim/Presentation architecture split, the dynamic weather system, testing strategy, and the M0–M5 milestone roadmap — now lives in **project_north_tech.md**. Headline architectural commitments: pure-C# deterministic simulation layer (headless-testable, seeded, forecastable weather), season as a scriptable state machine (the False Thaw is a state transition, built in from commit one), data-driven event layer for anomalies and plane beats, and versioned saves from the first write.

---

## 12. Open Questions — Working Agenda

**Resolved in v0.2:** lore framework (fictional Nordic cosmology, ecological wound, reclamation) · start season & plane logistics (Y1 spring, floats→skis, False Thaw of Y2, plane stops calendar-winter Y2) · economy model (hybrid cash→barter).
**Resolved in v0.3:** Morale/Resolve as fourth need · deep crafting direction (workbenches + ordered equipment; ammo/glass/metal producible) · dog confirmed (grandfather's dog missing; adopted partner) · player↔grandfather relationship (childhood summers, then silence) · calendar (28-day seasons, Stardew-style).
**Resolved in v0.4:** primer problem → regression-as-mastery (modern ammo Class F; craftable muzzleloader + black powder Class C) · failure states → difficulty toggle spectrum (lowest: food optional/energy-only; highest: permadeath).
**Resolved in v0.5:** tech stack (Godot 4.x Mono/.NET) · art direction (Stardew-style pixel art, denser) · dynamic spatial weather as CORE (storm hazard, diegetic forecasting).

Remaining, roughly in decision order:
1. **Project logistics (TECH §8) — mostly resolved:** solo dev, own art, PC/Steam only. Remaining: cadence/M1 horizon · pixel-density spec (dense-16 recommended; run the side-by-side test before mass art) · audio plan.
2. **Lore specifics (LORE doc §10):** naming register & glossary sign-off; the Wound (confirm mine); grandfather's death; the unfinished wardstone; observance verb set & backfire question; endings selection; the Vintermark's voice / the mine's interior marks; fate of grandfather's dog.
3. **Salt & the mineral site (§5.1):** Class F vs. Class C via a spring/lick — possibly the same site that supplies sulfur for black powder.
4. **Crafting spec pass (§5):** station list, recipe tiers, workshop power source (muscle/treadle/water/generator).
5. **Weather tuning (TECH §4):** exposure ladder rates per difficulty; fire-starting micro-game depth; whiteout navigation rules.
6. **Cartography depth (§5.3):** manual plotting minigame vs. streamlined; map desk required?; error visualization; can marks be wrong?
6. **Difficulty details (§5.2):** tier names, mid-game switching rules, per-tier tuning targets.
7. **Magic interactivity (§6.3)** — formally confirm rituals-not-spells (currently LEANING).
8. **Attunement trade-off (§6.2)** — pure key vs. cost.
9. **Settlement visibility & name (§4).**
10. **Region vibe & map scale (§9)** — shield vs. mountain vs. hybrid (weather system slightly favors having one hard ridge for storms to break against).
11. **Combat/danger pillar (§2)** — wildlife threat level; anything beyond wildlife?
12. **New dog details (§5):** how it arrives (pilot pup proposal), naming, capabilities scope.
13. **Fine calendar pacing (§3):** Act 3 length, exact plane-stop week (tunable in production).
