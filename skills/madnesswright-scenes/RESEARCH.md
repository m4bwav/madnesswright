# Research: madnesswright-scenes

Findings that back [SKILL.md](SKILL.md). Changes they caused are logged in [CHANGELOG.md](CHANGELOG.md); procedural lessons live in [LEARNINGS.md](LEARNINGS.md); test runs and their evidence in [TESTS.md](TESTS.md); schedule and state in `evergreen.json`. Protocol: MAINTENANCE.md.

Topic: MADNESS: Project Nexus level editing with the SceneTools Unity project: rooms, markers, proxies, Event System, stages, campaigns, arena stages, .sobj builds, encounter design and playtesting. Tier `moderate`. Last refresh 2026-10-17; next due 2026-11-16.

## Current understanding

- Settled: there is no in-game level editor. Levels are Unity scenes built in the developer's SceneTools project (Unity 2019.2.6f1 exactly) and exported as `.sobj` to `LocalLow/Gibbing Tree/Madness Project Nexus SceneTools/SceneObjs/` (R-20261017-1, read from AssetBundle_Builder_Editor.cs).
- Settled: the rule set (one Player_Start under Characters, never rotate rooms or Zone_Camera, Floor tag on Terrain and Static, Asset Label, one SceneRules in the root, 4-light cap, stage validity through SceneName and RequiredCharacters) comes from the official SceneTools Overview (R-20261017-2). `mpn.py scene-lint` checks the file-level ones; it passed both official sample scenes and caught a rotated room and a missing label in a mutated copy.
- Moving: Arena Siege and Infiltration tools shipped 2026-08-08 marked "testing"; custom arena stages can only replace existing ones; Dev Tools, Automatons and Asset Packs are roadmap items (R-20261017-3).
- Practice: encounter rules adapted from The Level Design Book, a 3D source; the side-view adaptation is ours (R-20261017-4).
- Unverified: the headless batch-mode build (no 2019.2.6f1 editor here); whether the game logs anything for a rejected stage.

## Open questions

- Will Arena HQ get free slots for custom arena stages (promised for the "near future")?
- Does a headless `-executeMethod` build work for the private menu methods, or only through ExecuteMenuItem?
- What does mods.txt say when a stage is dropped?

## Search plan

Four tracks; every refresh runs at least one query on each, scoped to the time since the last refresh (add the year). Protocol §4 explains the tracks.

Subject:

- Steam News API, the real changelog (the store's news page renders empty to fetchers): `https://api.steampowered.com/ISteamNews/GetNewsForApp/v2/?appid=488860&count=25&maxlength=0&feeds=steam_community_announcements`; page back with `enddate=<unix>`. Look for "beta", "modding", "SceneTools", "ModTools", "Workshop", version numbers.
- The official Google Docs as text: `https://docs.google.com/document/d/<id>/export?format=txt` (follow the 307 redirect) for the Modding Overview `1f_RbQGE45CY8no6mFuVGTaOTF7XnaK7yq5tJXty_JSA`, SceneTools Overview `1-fchWz0-kYVMUwZcM1RIST8VmWlQ-SmjcuSe2qz9EJI`, Event System `1aLz0hngXuPbw2UbXwU6yontyCwL7mzKk789GHfMTzJM`, Master Doc `1fgcDNarKoSe3I8CwaciqVxovZZJQlNQwTRUWRmvslN4`; diff against the last copy.
- GitHub API: `https://api.github.com/repos/MPN-Modders/MPN_SceneTools/commits` and `.../MPN_ModTools/commits` since the last check; `https://api.github.com/orgs/MPN-Modders/repos` for new repos.
- `"Madness Project Nexus" modding OR mod OR SceneTools <month> <year>`
- Official forums: Steam discussions `https://steamcommunity.com/app/488860/discussions/0/` and Workshop discussions `https://steamcommunity.com/workshop/discussions/?appid=488860`; announcements `https://steamcommunity.com/app/488860/eventcomments/`. The Discord (`discord.com/invite/projectnexus`) is not readable by fetchers; ask the user for pinned modding posts when a claim depends on it. Steam returns 429 to fast fetches: space them out or use fetchwright.

Tooling:

- `"Madness Project Nexus" mod OR modding OR tool site:github.com <year>`; `path:SKILL.md "Project Nexus"` and `path:SKILL.md "MadCard"` on GitHub code search
- `https://registry.modelcontextprotocol.io/v0/servers?search=unity` and `?search=ilspy` (decompile and runtime-inspection servers)
- `"BepInEx" OR "Harmony" skill OR plugin OR "mcp server" <year> site:github.com`; check rehan-remade/universal-modder for an MPN field note
- Supersession sweep: archive flags on MPN-Workshop-Uploader, mcilspy, universal-modder

Practice:

- `"Project Nexus" mod "Claude" OR "ChatGPT" OR "AI" <year>`; `"Madness Project Nexus" modding tutorial <year>` (YouTube titles count as leads)
- `site:steamcommunity.com "488860" "mods.txt" OR "not loading" OR "releasebeta"`
- `"Harmony" "Could not find method" game update <year>` for general Unity-mod agent failure reports

Testing:

- `"Madness Project Nexus" mods.txt OR "Player.log" error <year>`
- `unity mod smoke test BepInEx OR Harmony CI <year>`; `"LogOutput.log" "Chainloader" test`
- Level design playtesting: `book.leveldesignbook.com` changes; `"level playtest checklist" side-scroller <year>`

Best sources (primary first): Steam News API; the official Google Docs; github.com/MPN-Modders; Steam Workshop and discussions; the Modding Discord through the user. Noisy: generic "Nexus" searches (Nexus Mods app news), SteamDB (403), nexusmods.com (403 to fetchers), Newgrounds (403), SEO guide farms.

## Findings log

Newest first. One entry per material finding; a quiet refresh gets one entry saying so. `Track` is subject, tooling, practice, or testing.

### R-20261017-5 · 2026-10-17 · Tooling and testing
- Summary: No level tool for this game outside the official project; Unity MCP servers (CoderGamester/mcp-unity and others) target modern Editors and were not checked against 2019.2. No automated scene checker existed; mpn.py scene-lint was written and verified on the two official samples and a mutated copy.
- Track: testing
- Sources: https://registry.modelcontextprotocol.io/v0/servers?search=unity
- Magnitude: minor
- Applied: C-20261017-1 (Step 5)

### R-20261017-4 · 2026-10-17 · Encounter design and playtesting
- Summary: From The Level Design Book: one or two enemy types per encounter, introduce alone then combine, ramp by order and conditions, readable arenas, forgiving ambushes, beginning-middle-end waves, alternate risk and rest, resources as a difficulty lever, cover follows weapons; playtest method (focus questions, watch without helping, metrics, dominant strategy). A side-scroller postmortem: random spawn timing gave flat pacing.
- Track: practice
- Sources: https://book.leveldesignbook.com/process/combat/encounter, https://book.leveldesignbook.com/process/blockout/playtesting, https://drsronmars.itch.io/planet-null/devlog/182076/pacing-and-level-layout
- Magnitude: major (initial)
- Applied: C-20261017-1 (references/encounter-design.md)

### R-20261017-3 · 2026-10-17 · Feature timeline
- Summary: SceneTools public 2024-11-02 (Playground only); custom campaigns and cutscenes 2025-08-06; Vessels and Custom Seats 2026-04-15; custom Arena Siege and Infiltration 2026-08-08 (testing). Showcases: DarkSignal's Nevadean Supermall (Workshop 3536852525), Tower of MADNESS: Evolved.
- Track: subject
- Sources: Steam News API
- Magnitude: major (initial)
- Applied: C-20261017-1 (Step 2)

### R-20261017-2 · 2026-10-17 · SceneTools and Event System docs
- Summary: SceneTools Overview: setup (_PG suffix, Room_Layout, SYNCH VISUALS, Dev_Cam), floors and layers, camera layout, Player_Start and Waypoint rules with placement guidance, proxies, markers, zones, Squad Spawner, layers, removers, blockers, custom SceneObjects, campaign and stage card rules, cutscenes, SceneRules arena additions. Event System Overview: EventCards (no Root, first Filename wins), Conditions and Actions, AmCheckpoint, TrueForever, Booleans vs WorldChanges, Tasks, SceneRules registration, Missions.
- Track: subject
- Sources: https://docs.google.com/document/d/1-fchWz0-kYVMUwZcM1RIST8VmWlQ-SmjcuSe2qz9EJI, https://docs.google.com/document/d/1aLz0hngXuPbw2UbXwU6yontyCwL7mzKk789GHfMTzJM
- Magnitude: major (initial)
- Applied: C-20261017-1 (Steps 3 to 5, references)

### R-20261017-1 · 2026-10-17 · Creation pass: SceneTools project
- Summary: Shared creation pass (cost line in madnesswright R-20261017-1). github.com/MPN-Modders/MPN_SceneTools, 27 commits, newest 2026-09-02 (Uniforms on Marker_Character), 2026-08-03 (Blockers, Arena SceneRules, Siege). Unity 2019.2.6f1 (ProjectVersion.txt). Build menu `Madness/AssetBundle/ >> Build (Compressed) *RECOMMENDED*` and `(Uncompressed)`, output persistentDataPath/SceneObjs, StandaloneWindows64; non-scene bundles are deleted. Scenes are text YAML; layer indices from TagManager.
- Track: subject
- Sources: https://github.com/MPN-Modders/MPN_SceneTools
- Magnitude: n/a (initial)
- Applied: C-20261017-1
