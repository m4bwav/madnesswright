# Changelog: madnesswright-scenes

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

### C-20261116-c · 2026-11-16 · raised floors: any-direction ramp, waypoint lift, fake null
- because: user request (Nexus Prominence build zzj, the requester's "no ramp up to the pylons, no steps next to a door")
- files: references/raised-floors.md (Built so far)
- Editor-side; the two new walkable pieces are unplayed.

### C-20261116-b · 2026-11-16 · scene-lint S26 coplanar faces, S27 narrow gaps
- because: user request (the requester's glitch shots of Nexus Prominence build zzg)
- files: references/scenetools-rules.md (S26, S27); scripts/geometry.py (madnesswright skill)
- Editor-side checks; the fixes they led to in zzh are unplayed.

### C-20261116-a · 2026-11-16 · learning: film structures clear side doors
- because: L-20261116-1 `film-structures-clear-side-doors` (Nexus Prominence builds zze and zzf failed the door gate on first pass with corner pieces)
- files: LEARNINGS.md

### C-20261115-a · 2026-11-15 · scene-lint S22 outrun squads; cap claim corrected
- because: a player's report on Nexus Prominence build zu (backup met only on walking back); decompiled Squad_DataAssigner.Update, Wave_Manager.CheckReadyToSpawn, Wave_Tandem.CanSpawnWithoutBreachingMax
- files: references/scenetools-rules.md (S22, the cap, the timer); scripts/staging.py and mpn.py (madnesswright skill); tests/test_mpn.py
- Corrects the 2026-11-14 claim that placed enemies hold back a first squad. Editor-side; the fix is unverified in game.

### C-20261114-r9 · 2026-11-14 · scene-lint S21 sample wall mesh still drawn
- because: Nexus Prominence ten rounds, round 9 (the Playground room, second pass); decompiled Room_Main.MakeChildrenVisible (lights, terrain, backgrounds only)
- files: references/scenetools-rules.md (S21); scripts/staging.py (madnesswright skill)
- Editor-side; a hidden room renderer in play is backed by the 4th wall, hidden the same way in played builds.

### C-20261114-r6 · 2026-11-14 · scene-lint S19 enemy staging, S20 FixedInPlace freeze
- because: Nexus Prominence ten rounds, round 6 (every room the same fight); decompiled Converter_Marker, Char_DataAssigner, AI_Modules.M_SniperCamp, Controller_NPC behaviour-to-module map
- files: references/scenetools-rules.md (S19, S20); scripts/staging.py (madnesswright skill)
- Editor-side; whether the behaviours read in play is unverified.

Entry shape: `### C-20261114-r5 · 2026-11-14 · scene-lint S18 box share
- because: Nexus Prominence ten rounds, round 5 (care and appeal); build zo log measured six code-made OBJ props imported 1:1
- files: references/scenetools-rules.md (S18); scripts/mpn.py box_share_check (madnesswright skill)
- Editor-side measure; how players read it is unverified.

### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

### C-20261114-2 · 2026-11-14 · Scene-lint S16 (the Playground control room proxy) and what PX_Playground becomes in game
- because: user request (Nexus Prominence ten rounds, round 2: a player reported "a playground room for no reason"); decompiled game (Converter_Proxy, InteractiveMode, Stage_Manager)
- files: references/scenetools-rules.md (proxy table PX_Playground row; Hard rules S16); mpn.py scene-lint S16
- PX_Playground is the whole control room booth, not just a window, and a map loads without it. Unverified in play; SKILL.md unchanged.

### C-20261114-1 · 2026-11-14 · Scene-lint S15 (unused spawn doors) and how a Squad_Spawner fires
- because: user request (Nexus Prominence ten rounds, round 1: a player reported enemy entrances that are never used); decompiled game build 25615196
- files: references/scenetools-rules.md (Hard rules S15; new section "How a Squad_Spawner fires"); mpn.py scene-lint S15
- The cap rule is read from the game's code, unverified in play; SKILL.md unchanged.

### C-20261112-6 · 2026-11-12 · SceneTools component fields from the test repo tooltips
- because: user request (technique harvest); ai-docs/notes/2026-11-12g-madness-technique-harvest.md items 81, 83-99
- files: references/scenetools-fields.md (new); references/scenetools-rules.md (pointer before "Build output"); references/event-actions.md (RunDynamicMethod and RunLua: float and vector syntax)
- Tooltip text from MPN_SceneTools_Testing, unverified in game; item 81's sub-fields were already in event-actions.md and matched the sheet; SKILL.md unchanged.

### C-20261112-5 · 2026-11-12 · Full Event Conditions and Actions, BooleanSet fix, early SceneTools objects
- because: user request (technique harvest); ai-docs/notes/2026-11-12f-madness-technique-harvest.md items 72-80
- files: references/event-actions.md (new); references/stages-and-campaigns.md (Events: BooleanSet, pointer, briefing voice); references/scenetools-rules.md (Beta SceneTools additions: earlier objects)
- Official sheet or developer commit text, unverified in game; SKILL.md unchanged.

### C-20261112-4 · 2026-11-12 · Beta SceneTools additions (water, fog, vessels, seats, doors, blockers, movie player)
- because: user request (technique harvest); ai-docs/notes/2026-11-12e-madness-technique-harvest.md items 61-65, 67-71
- files: references/scenetools-rules.md (new "Beta SceneTools additions"); references/stages-and-campaigns.md (end)
- Beta-only commit messages, field names unconfirmed, unverified in game; SKILL.md unchanged.

### C-20261112-3 · 2026-11-12 · Debug mode for playtests
- because: user request (technique harvest); ai-docs/notes/2026-11-12c-madness-technique-harvest.md item 50
- files: references/encounter-design.md (new "Debug mode for playtests")
- Single source, unverified in game; SKILL.md unchanged.


### C-20261112-2 · 2026-11-12 · Stage music pointer
- because: user request (technique harvest); ai-docs/notes/2026-11-12b-madness-technique-harvest.md item 28
- files: references/stages-and-campaigns.md (end)
- Points stage music at the modtools sounds reference; unverified in game.

### C-20261112-1 · 2026-11-12 · Door parenting (S14), floor at the back wall, the bib, Squad_Spawner fields
- because: user request (technique harvest); ai-docs/notes/2026-11-12-madness-technique-harvest.md items 23-26 (community video _xbtGGh6Rso)
- files: SKILL.md (Step 2 geometry, Step 4 doors); references/scenetools-rules.md (PX_Door and Squad_Spawner rows, Hard rules S14, new sections "Squad_Spawner fields" and "Hiding the void"); mpn.py scene-lint S14
- Confidence kept as the note gives it; the door rule, spawner placement, stockpile cap and bib shadow fix are unverified in game.

- Older entries (C-20261017-1 to C-20261111-9): [ai-docs/archive/scenes-changelog-to-2026-11-11.md](../../ai-docs/archive/scenes-changelog-to-2026-11-11.md).

### C-20261111-1 · 2026-11-11 · Cosmetic shootable props
- because: L-20261111-3 (owner play of the Nexus Prominence breakables, user request "improve the madnesswright")
- files: references/scenetools-rules.md, LEARNINGS.md
- Destructible recipe for look-only set pieces, confirmed in game.

### C-20261111-2 · 2026-11-11 · Toggle automators need a custom Togglable
- because: L-20261111-4 (Player.log NREs in the Nexus Prominence spike play, user request "improve the madnesswright")
- files: references/scenetools-rules.md (Custom SceneObject line), LEARNINGS.md
- Automator placement rule and the driver route for timing stock spikes.

### C-20261111-3 · 2026-11-11 · Rules from the official stages and cutscenes video
- because: R-20261111-3 (harvest, user request "do some more technique harvesting")
- files: references/scenetools-rules.md (six lines under "Spawns, doors and moving parts"), references/stages-and-campaigns.md (stage card, Hubs, Stage marker, cutscene fields)
- SKILL.md is unchanged, so triggers and decisions stay the same and the evals do not need a rerun.

### C-20261114-3 · 2026-11-14 · Textures, prop import scale and film cameras
- because: measured in Nexus Prominence round 3 (build zm): OBJ in metres imports 1:1 (bounds logged), tiling on Terrain - General needs metre UVs, pw.py --seamless ghosts grid patterns
- files: references/textures-and-props.md (new), SKILL.md (one pointer sentence in step 2)
- Triggers and decisions unchanged; evals not rerun.

### C-20261114-4 · 2026-11-14 · Player start and door headers
- because: L-20261114-12 `player-start-needs-floor` and L-20261114-13 `door-headers-back-wall-only`, both found playing Nexus Prominence after round 10 (builds zt, zu; zu released)
- files: LEARNINGS.md only. Triggers and decisions unchanged; evals not rerun.

### C-20261116-1 · 2026-11-16 · Raised floors reference
- because: R-20261116-1 (user request: record how the game treats a floor at a second height)
- files: references/raised-floors.md (new), SKILL.md (link in the geometry step); mpn.py scene-lint S24 info line. Triggers and decisions unchanged; evals not rerun.

### C-20261116-2 · 2026-11-16 · Spikes that never hurt, were dodged, never rose
- because: user requests on Nexus Prominence zzh, zzi, zzk (spikes never hurt; still never hurt; hurt but never rise); decompiled Converter_Proxy, Toggleable_Main (Up/Down objects only SetActive'd; a re-enabled Animator resets), Hazard_Core/Hazard_Main/Combat_CharacterHit (tac bar dodges unless HitAvoid -1).
- files: references/scenetools-rules.md (timed spikes via LinkedAnimator plus a Hazard_Core trigger, HitAvoid -1, IgnoreThreshold, layer 12; S28-S31 listed). Triggers and decisions unchanged; evals not rerun.
