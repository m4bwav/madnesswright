---
name: madnesswright-scenes
description: "Build levels for MADNESS: Project Nexus with the official SceneTools Unity project (Unity 2019.2.6f1): Playground scenes (_PG), rooms from Room_Layout, floors, waypoints, doors, cover, lockers, spawners, zones, blockers, the Event System, custom stages, campaigns, cutscenes and Arena Siege or Infiltration stages, exported as .sobj. Checks a scene against the official rules with mpn.py scene-lint (one Player_Start under Characters, never rotate rooms, Floor tag on Terrain, Asset Label, one SceneRules), plans encounters and waves, and runs the build-deploy-playtest loop. Use whenever the user asks to 'make a level', 'build a map', 'custom stage', 'Playground scene', 'arena map', 'custom campaign', 'level editor', 'my scene doesn't show up', 'enemies don't path', 'design the waves', or names SceneTools, .sobj, Room_Layout or Squad Spawner. MadCards, branch and mod setup are madnesswright; models and code are madnesswright-modtools."
---

# MADNESS: Project Nexus levels (SceneTools)

Outcome: a scene that `mpn.py scene-lint` passes, built to a `.sobj`, placed in a mod that `mpn.py check-mod` passes, and a playtest report from the user against a written checklist. There is no level editor inside the game (2026-10): levels are Unity scenes made in the official SceneTools project.

`MPN` means `python <madnesswright skill>/scripts/mpn.py` (the sibling skill folder `../madnesswright/scripts/mpn.py`).

## Step 0: freshness (every use, one read)

Read `evergreen.json` next to this file. If `contradiction` is set or today is on or after `next_due`, say so in one line, do the task, then refresh (`evergreen-refresh`) in the same session. If `tests.failing` is non-empty, say so and run `evergreen-tune` after. SceneTools is work in progress; features and rules change with game betas.

## Step 1: setup

1. The game must be on the `releasebeta` branch with a fresh backup (madnesswright Step 2: `MPN where`, `MPN backup`).
2. Get the project: clone or download `https://github.com/MPN-Modders/MPN_SceneTools` (the developer's own; no license file, so do not redistribute its files). Pull it again before a session: commits land every few weeks (Arena tools 2026-08-03, Uniforms on Marker_Character 2026-09-02).
3. Install exactly Unity 2019.2.6f1 from the Unity download archive through Unity Hub (Windows build support). Another Unity version upgrades the project and its bundles will not load in the game. The user installs Unity; the agent does not.
4. Open the project root (the folder holding Assets, Packages and ProjectSettings) in Unity Hub. Start from `Assets/Scenes/SamplePlayground_PG.unity` or `SampleSiegeArena.unity`: copy one, do not edit the samples.

## Step 2: decide what is being built

Ask or infer, then say it in one line:

- **Playground scene**: name ends `_PG`; appears in the Playground scene list (terminal by the control room door). The fastest test loop; build here first even for a stage.
- **Custom stage in a campaign**: Story rules; needs a Stage MadCard and a Campaign MadCard. No Siege or Infiltration in campaigns.
- **Arena Siege or Infiltration stage**: SceneRules arena options; as of 2026-08 a custom arena stage can only replace an existing Arena stage, and the tools are marked "testing".

Card fields for stages, campaigns, cutscenes, events and missions: [references/stages-and-campaigns.md](references/stages-and-campaigns.md).

## Step 3: block out the room

Follow [references/scenetools-rules.md](references/scenetools-rules.md) (objects, layers, hard rules). The order that avoids most rework:

1. New scene; delete Main Camera and Directional Light. Drag in `Room_Layout`; click SYNCH VISUALS (makes the Dev_Cam). Never rotate a room.
2. Geometry from primitives first: floor (Tag Floor, Layer Terrain, Static), back walls Terrain, foreground pieces TerrainObstacle, the camera-side wall TerrainInvis or no renderer, an invisible ceiling collider. Stairs need an invisible Interactive-layer ramp.
3. Camera: Camera Point and Camera Target, the four Border markers, Leeway; SYNCH VISUALS after each change. The Dev_Cam's own camera values do not carry into the game.
4. Exactly one `Player_Start`, parented to the room's `Characters` object. At least one Waypoint per room, at floor level, under `Waypoints`, placed so every area is seen by one waypoint; add corner waypoints around large geometry.
5. Light: `Light_Main` in the room; at most 4 lights active (3 on low quality); use Glow Effect prefabs for extra light sources.

## Step 4: fill it

Doors (PX_Door, linked between rooms), cover (PX_Cover), lockers, interactives, enemies (Char_Start for placed characters, Squad Spawner for waves through doors), Item_Spawn, Spawnpoints (floating, for the Playground item spawner), zones, blockers, removers by difficulty. Plan encounters with [references/encounter-design.md](references/encounter-design.md) before placing enemies: write the encounter plan (per room: enemy types, count, entry doors, cover, the beat it plays) into the mod folder as `LEVEL-PLAN.md`, so the playtest has something to check against.

## Step 5: lint, build, deploy

1. Save the scene (Ctrl+S); Unity writes it as text YAML. Run `MPN scene-lint <path/to/Scene.unity>`. Its JSON lists rule ids (S01 to S13) with the fix; fix every error, read warnings to the user. It checks the scene file only: it cannot see in-game behaviour, pathing or event logic.
2. Give the scene an Asset Label (bottom of the Inspector with the scene asset selected). Scenes and prefabs cannot share a bundle.
3. Build: menu Madness, AssetBundle, `>> Build (Compressed) *RECOMMENDED*` (the menu's own advice; the 2024 doc says compressed loads slower, so offer Uncompressed when load time matters). Output: `LocalLow/Gibbing Tree/Madness Project Nexus SceneTools/SceneObjs/<label>.sobj`; `MPN where` lists the newest under `tool_outputs.scenetools`. Unity's Console shows "Exported Successfully" or a warning that a bundle had no scene.
4. Copy the `.sobj` into the mod folder (with any Stage, Campaign, Event or Cutscene cards), then `MPN check-mod` and `MPN deploy`.

Headless build (unverified, no 2019.2.6f1 editor was available to test): add an editor script with a public static method that calls `EditorApplication.ExecuteMenuItem("Madness/AssetBundle/ >> Build (Compressed) *RECOMMENDED*")`, then run `Unity.exe -batchmode -projectPath <SceneTools> -executeMethod <Class>.<Method> -quit -logFile -` and read both stdout and the log for errors. Say it is unverified when offering it.

## Step 6: playtest and iterate

1. The user enables the mod (gear icon, ENABLED, Apply), opens the Playground terminal, and loads the scene (or starts the custom campaign from its menu entry).
2. Give the user the checklist from [references/encounter-design.md](references/encounter-design.md#playtest-checklist) filled in for this level: spawn point, camera borders, each door, enemy pathing to every area, each encounter clearable, deaths and time per room.
3. After the session, run `MPN log` and report problem lines; take the user's notes, change one thing at a time, lint, rebuild, redeploy.
4. Dev Tools (god mode, AI pathing overlay) are on the developer's roadmap, not shipped: debug pathing by watching enemies and adding waypoints or ForceConnect.

## Rules that bite

- Never rotate rooms or Zone_Camera; Zone_Camera must sit under the room's `Waypoints` object.
- A stage with no valid `SceneName` or no `RequiredCharacters` is dropped; do not reuse one Character card twice in a player squad.
- Lights outside rooms are ignored; the Background object only shows for the focused room.
- Events last one scene; use WorldChanges for anything that must persist. EventCards have no Root and do not override: the first card with a Filename wins.
- Arena start interactive: do not give it an `Arena_StartWaves` action; the game adds it.

## Maintenance

This skill is evergreen: state in `evergreen.json`, research in [RESEARCH.md](RESEARCH.md), edits in [CHANGELOG.md](CHANGELOG.md), lessons in [LEARNINGS.md](LEARNINGS.md), tests in [TESTS.md](TESTS.md). Protocol: [MAINTENANCE.md](MAINTENANCE.md).
