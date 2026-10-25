---
name: madnesswright
description: "Mod MADNESS: Project Nexus (the 2021 Steam game, not the Flash classic) the official way: releasebeta branch with a save backup first, a mod folder under LocalLow/Gibbing Tree/Madness Project Nexus/Mods with info.txt, MadCards (.json container or .txt shorthand) and .mobj/.sobj bundles, enabling it from the gear icon, proving it loaded from Mods/mods.log, and sharing it on the Steam Workshop. Bundled mpn.py finds the install and branch, backs up saves, lints and deploys a mod, and reads the logs. Use whenever the user asks to mod Project Nexus or MPN, 'make a MadCard', 'give Hank more health', 'green blood mod', 'why isn't my mod loading', 'install this mod', 'upload to the Workshop', 'which branch do I need', or mentions MadCards or Madness Combat modding. Any level, map, Playground scene, .sobj, stage, arena or campaign request goes to madnesswright-scenes instead; 3D weapons, armor, characters and Harmony code are madnesswright-modtools. Windows; Linux and Steam Deck through Proton."
---

# MADNESS: Project Nexus modding

Outcome: a mod folder that `mpn.py check-mod` passes, deployed into the game's Mods folder, with `mpn.py log` output showing how it loaded, and the user told the one thing to look for in game. The agent cannot see the game; the logs and the user's report are the evidence, never the agent's say-so.

`MPN` below means `python <this skill>/scripts/mpn.py` (`python3` on macOS and Linux). Every command prints one JSON object.

## Step 0: freshness (every use, one read)

Read `evergreen.json` next to this file. If `contradiction` is set or today is on or after `next_due`, say so in one line, do the task with the current content, then refresh (`evergreen-refresh`) in the same session. If `tests.failing` is non-empty, say so and run `evergreen-tune` after the task. Branch names, versions and tool features below are dated; the game patches often.

## Step 1: right game, right route

- MADNESS: Project Nexus (2021, Steam 488860, developer and publisher GIBBING TREE, LLC) is this skill. "Madness: Project Nexus (Classic)" is the older Flash game on Newgrounds with unrelated saves and mods; say so and stop if that is what the user means. The Steam install folder is still named `Madness Project Nexus 2` (pre-release title) and the data folder `Madness Project Nexus_Data`.
- Pick the lowest rung that does the job and name it in one line:
  1. MadCards only (text): stats, names, blood colour, loadouts, waves, stage values, swapping one model for another (`"ObjRef": "*M_Pipewrench"`). This skill.
  2. New art (models, textures, icons, animations, characters): ModTools project, `.mobj`. madnesswright-modtools.
  3. New rooms, Playground scenes, stages, campaigns, cutscenes, arena stages: SceneTools project, `.sobj`. madnesswright-scenes.
  4. New behaviour no card exposes: a Harmony assembly. madnesswright-modtools.
  5. Legacy routes (replacing `Assembly-CSharp.dll`, BepInEx builds of Not Enough Madness, UABE texture swaps in `sharedassets`): only for old release builds (1.08-era). They break on every update and are superseded; use them only if the user insists, after a backup of the whole `_Data` folder.

## Step 2: check the setup

1. Run `MPN where`. It reports the install, `build_id`, `branch` (Steam `BetaKey`), the data folder, the Mods folder, `mods_log`, `enabled` (the game's enabled list), `Player.log`, `game_version` (from the log's `GAME INITIALIZED:` line), and the SceneTools and ModTools build output folders. Quote the branch and the Mods path back to the user.
2. Official modding runs on the `releasebeta` branch only. The release build carries the mod loader but switches it off unless the build reports itself as Beta (release v1.11.a, build 25615280, read 2026-10-17); `game_version.mods_allowed` says which build ran last. If `branch` is not `releasebeta`: first run `MPN backup` and confirm the zip path it prints, then have the user switch in Steam (Library, right-click the game, Properties, Betas, `releasebeta`, no password) and launch once.
3. Before switching branches, removing or renaming a mod that adds items, or replacing any game file, run `MPN backup` again. Removing a mod once wiped modded weapon inventories (fixed in beta 2.13.l, 2024-12-19); saves are cheap to keep.
4. No install found: pass `--steam <root>` or `--data-dir <folder>`. On Linux and Steam Deck the data folder is inside the Proton prefix (`steamapps/compatdata/488860/pfx/drive_c/users/steamuser/AppData/LocalLow/Gibbing Tree/Madness Project Nexus`). There is no native macOS or Linux build.

## Step 3: write the mod

A mod is one folder inside `Mods/`: `info.txt` (or `info.json`) with a non-empty `Name` and `Description` (the game skips the mod otherwise), an optional image named `preview`, any number of MadCard files, any `.mobj` or `.sobj` bundles, and an optional assembly. Read [references/madcards.md](references/madcards.md) before writing cards; it has the formats, the inheritance rules and working examples.

- Write straight ASCII quotes. The official Google Docs examples carry curly quotes; the game straightens curly double quotes in `.txt` cards only, so in `info.txt` and `.json` they break the file. `check-mod` flags them.
- `.json` files are strict JSON in the container layout `{"<Type>": [cards]}`. The game silently ignores a bare card in a `.json` file and any list name it does not know (`Characters` for `Character`). Shorthand (cards at top level, each with `"Type"`) must be saved as `.txt`; there `Type` is case-sensitive.
- Every card needs `Filename` and `Type`. A Filename the game or another mod already uses overrides that entity; a new Filename adds one. Use `Root` to inherit everything you do not change.
- Never invent a Filename, Type or field name. The authoritative glossary is the MadCard Maker app (MCM, by DarkSignal, linked from the Modding Resources Master Doc) and Gibbing Tree's official asset dumps. When the user has neither open, write the card with the names you are sure of from the docs and mark every other name as "confirm in MCM". A misspelt Root or Filename does not crash anything: the card silently does nothing or makes a new entity.
- `Dependencies` in info.txt are Steam Workshop item ids only; local mods cannot depend on other local mods.

## Step 4: check and deploy

1. `MPN check-mod <mod-dir>`. Fix every error; read the warnings to the user. The `info.card_list` it prints is what the game will see.
2. `MPN deploy <mod-dir>` copies the folder into `Mods/` after the same check (refuses on errors or an existing folder unless `--force`). Its JSON (`"deployed": true`, `"dest"`) is the evidence. If the user works directly inside `Mods/`, skip the copy.

## Step 5: enable and verify

1. The user enables it: main menu, gear icon at the bottom left, select the mod, tick ENABLED, then Apply. No gear icon means the build is not Beta (the release build deletes the button); the main-menu version text should start with Beta. Once `Mods/enabled.txt` exists, a newly deployed folder starts disabled until enabled here. Mods are read at launch: `MPN deploy --force` can replace a mod while the game runs, and the change loads on the next restart.
2. After the user has started a mode that uses the mod, run `MPN log`. `mods_log` is the game's own mod log, `Mods/mods.log` (the official doc calls it `mods.txt`; the game writes `.log`): `loaded` lists the enabled mods, `applied_harmony` the patches, `problems` the `FAILED` and `ERROR` lines. `player_log` is Unity's. Report the problem lines for this mod by name, or say plainly that none appeared.
3. Tell the user exactly what to look for in game (the new weapon in the Playground spawner, the agent's health bar, the blood colour) and ask them to report it. Done means a clean log plus the user's confirmation.

Troubleshooting order is in [references/troubleshooting.md](references/troubleshooting.md).

## Step 6: share it

- There is no uploader inside the game (2026-10; the beta's code only reads Workshop items). Community uploaders: MPN-Workshop-Uploader (github.com/placenamehereig/MPN-Workshop-Uploader, MIT, interactive console on Facepunch.Steamworks; its content path must contain a dot, so name the folder like `MyMod.content`) and Soup-o-Stat's GUI uploader (no license). They upload through the Steam client the user is logged into, so no password is typed; visibility is set by the tool or on the item page. For an upload an agent can run, see the repository's examples/WorkshopUpload (arguments, no prompts). The user runs or approves the upload with their own account; the agent never handles Steam credentials.
- Workshop items take their title and description from the Workshop page, so a card-only item needs no `info.txt`; an item with an assembly still ships one for `AssemblyPath`. Set "Required Items" for every dependency on the Workshop page, and pick the Workshop's own tags (arena, characters, weapons, armor, enemies, playground, custom-stage, story, scripts, QOL and others).
- Every update gets change notes on the item's Change Notes tab: the version and what changed for the player since the last update, from the mod's changelog. Subscribers read them there; a bare "Update" tells them nothing. examples/WorkshopUpload takes them as a file; with another uploader, fill them in with Edit on that tab.

## Safety

- A mod with an assembly runs code on the player's PC. Before enabling someone else's DLL mod, say so; Windows Defender has flagged mod DLLs before. Prefer Workshop items with readable source.
- The game has no online multiplayer or anti-cheat (local co-op and Remote Play only), so mods affect only the local machine.

Sources, links and community hubs: [references/sources.md](references/sources.md).

## Maintenance

This skill is evergreen: state in `evergreen.json`, research in [RESEARCH.md](RESEARCH.md), edits in [CHANGELOG.md](CHANGELOG.md), lessons in [LEARNINGS.md](LEARNINGS.md), tests in [TESTS.md](TESTS.md). Protocol: [MAINTENANCE.md](MAINTENANCE.md).
