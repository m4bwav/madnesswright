# Changelog: madnesswright-scenes

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

Entry shape: `### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

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

### C-20261111-9 · 2026-11-11 · Harvested SceneTools details: stockpiles, door states, platforms, cutscene cameras
- because: user request (technique harvest); ai-docs/notes/2026-11-11-level-technique-harvest.md (official SceneTools and Event System docs via the pinned Master Doc)
- files: references/scenetools-rules.md (new section "Spawns, doors and moving parts"); SKILL.md (Maintenance links harvest.json)
- Five rules stated by the doc author; seats, AnimEvents_Interact and vendor currency left in the note.

### C-20261111-8 · 2026-11-11 · Session lessons distilled: frames, one change per play, clear wall, weapon names, headless build verified
- because: user request (technique distillation); L-20261111-9, L-20261111-10, L-20261111-11; ai-docs Owed 7 (headless build done 2026-11-10)
- files: SKILL.md (Step 3 item 0, Step 4 weapons and decals, Step 5 headless build, Step 6 item 3); LEARNINGS.md
- Headless build is no longer "unverified": it names the reflection calls NexusBatch.cs uses. Added: rooms from video frames, weapon card names via UnityPy, decals on a clear wall patch, no stacked size/camera changes.

### C-20261111-7 · 2026-11-11 · Community harvest targets for level design
- because: user request (technique harvest and scout sources for MPN level design)
- files: harvest.json (new)
- Five targets, each URL checked on 2026-11-11 (HTTP 200): GameBanana MPN section, Steam discussions (pinned Modding/Workshop Resources Master Doc, topic 4328600953645693620), Steam guides (The Definitive M:PN Modding Guide, id 3305980802), r/madnesscombat (low; reddit.com is not fetched directly), a YouTube search for SceneTools level tutorials. The official Discord is a source to read by hand only: it cannot be scraped, so it has no target. A guide titled "Modder's Guide to Madness" was not found and is left out.

### C-20261111-6 · 2026-11-11 · Game shader and solid props for generated geometry
- because: L-20261111-6 (Workshop feedback on the Nexus Prominence map)
- files: SKILL.md (Build steps, step 2); LEARNINGS.md
- Step 2 now names the `Madness/Terrain - General` shader and colliders on solid-looking props.

### C-20261023-2 · 2026-10-23 · The last wave debuts nothing new; template columns carry the limits
- because: T-20261023-1 (first round: all three runs debuted two new roles in the final wave without a word; second round: one run set "Max attacking 3", one debuted two roles in wave 4), L-002
- files: references/encounter-design.md ("Wave and squad pacing"); references/level-plan-template.md (room and wave columns); evals/evals.json (outcome-2 expectation and evidence)
- The last wave uses only roles already met, and a brief with more new roles than waves is flagged with another wave or a move to the next stage offered. The column headers now say "one at most" and "1 or 2 attacking at once". The outcome-2 cap regex no longer matches a bare "simultaneously".

### C-20261023-1 · 2026-10-23 · Enemy roles, orchestration, wave pacing, finale and fairness rules; LEVEL-PLAN template
- because: books track 9 (Rogers, *Level Up!* 3rd ed., Levels 4 and 9 to 13 and 19, notes in the owner's book-learnings folder); T-20261023-1
- files: references/encounter-design.md (new sections "Enemy roles", "Orchestration order", "Wave and squad pacing", "Boss or finale room", "Fairness"; playtest checklist items 9 and 10); references/level-plan-template.md (new); SKILL.md (Step 4); evals/evals.json (outcome-2)
- Roles are decided per enemy card by watching it alone in a Playground arena: the game's cards sit inside compiled bundles, so no card-to-role mapping is asserted. The round length of 3 to 5 minutes is marked as a heuristic. The template holds a room table, a role-per-card table, a wave table, playtest results and a campaign beat chart whose music row musicwright-game can read.

### C-20261017-2 · 2026-10-17 · Route level and map requests to madnesswright-scenes
- because: T-20261017-4 (madnesswright-scenes 0/6 on its trigger runs; the hub and modtools took them)
- files: SKILL.md (description)
- Leads with 'make, fix or debug a level, map or stage', says there is no in-game editor, adds 'my Playground map doesn't show up' and 'I built the sobj', says level requests start here, and shortens the rule list to stay under 1,024 characters.

### C-20261017-1 · 2026-10-17 · Created as an evergreen unit
- because: user request
- files: SKILL.md, RESEARCH.md, LEARNINGS.md, evergreen.json (skills: also TESTS.md and evals/evals.json)
- Initial version. Tier `moderate`, interval 30d. See R-20261017-1 for the research basis; the first test run, for a skill, is logged in TESTS.md.

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
