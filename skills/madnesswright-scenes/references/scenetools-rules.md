# SceneTools objects, layers and rules

Source: the official SceneTools Overview Google Doc and the MPN_SceneTools repository (TagManager, AssetBundle_Builder_Editor.cs, sample scenes), read 2026-10-17. Ids S01 to S13 are the checks `mpn.py scene-lint` runs.

## Terms

- **Scene**: a Unity scene; a loading screen in the game is a scene change. One `.sobj` can hold many scenes; `.sobj` bundles hold scenes only.
- **Room**: a gameplay area; `Room_Layout` prefab with a RoomObj script, children `Characters`, `Waypoints`, `Background`, `Light_Main`, camera markers.
- **ProxyObject** (`PX_`): stands in for a game object that "does something"; replaced by the real one in game. Type dropdown picks the variant.
- **MarkerObject**: invisible features (pathing, spawns, zones).
- **Custom SceneObject** (`CO_` prefabs): Cover, Togglable, Interactive (+ Locker or Vendor addon), Destructible, Door, Seat built from scratch.

## Objects

| Object | Use | Rule |
|---|---|---|
| Player_Start | where the player starts; SpawnFromDoor links it to a door | exactly one per scene (S01), under the room's Characters (S02) |
| Char_Start | one placed character by MadCard Filename, with Held, Stow, Thrown, Worn item cards | SpawnEnabled false + a Zone_Spawner to spawn later |
| Waypoint | AI path node | at least one per room; at floor level unless Floating; under Waypoints |
| Sniperpoint | waypoint snipers prefer | |
| Spawnpoint | off-screen spawns; Playground item spawner | place a few floating |
| Item_Spawn | one specific weapon on the floor | |
| PX_Door | doors, manholes, rappel lines; links rooms and scenes | every custom door needs a StartPoint |
| PX_Cover, PX_Locker, PX_Interact, PX_Togglable, PX_Seat | cover, weapon chests, switches and terminals, moving geometry, seats | interactives need a Task to do anything |
| PX_Playground | the Playground control room window | ApplyWindowMask if the wall has no cut-out |
| PX_SiegeRepair | wood for repairing Siege destructibles | Arena |
| Zone_Event, Zone_Dead, Zone_OffLimits, Zone_Spawn(er), Zone_StagingGround | events, kill volumes, teleport-back, triggered spawns, arena squad conscription | |
| Zone_Camera | camera override while the player is inside | never rotated (S04); under the room's Waypoints (S05) |
| Squad_Spawner | waves through doors: Units (Count, GenerationScale), Squad and Max, Weapon Stockpiles, Bosses, Custom Squads | position does not matter |
| Blocker_Progress / Blocker_Backtrack | lock the player in until spawners finish / after entering | point Z-forward toward where the player arrives |
| Cutscene | marker + CutsceneCard; Actors by SerialNumber (`PSQUAD0`...), Marks, ViewCams | |
| SceneRules | registers EventCards for the scene; arena start interactive, Siege targets, victory cutscenes | at most one (S06), in the scene root (S07) |
| Removers | omit objects by difficulty, random chance, origin, arena upgrade, wave checkpoint, WorldChange | go on anything, rooms included |

## Layers (index in the project's TagManager)

Terrain (8) walls and floors, blocks everything including aim. TerrainObstacle (28) like Terrain but the cursor aims through it: use for foreground pieces. TerrainInvis (15) like TerrainObstacle without bullet holes or blood: the camera-side wall. Obstacle (13) small cover and columns: blocks bodies, not sight; waypoints connect through it. Interactive (12) blocks standing characters only: invisible stair ramps. CharacterBlock (21) also blocks ragdolls. Fence (18) blocks bodies, not bullets or sight. ObjectBlock (20) blocks loose physics props. PathBlock (25) blocks only AI pathing and waypoint links: cliff edges. BulletBlock (26), SightBlock (31), BlockAim (24, catches the cursor outdoors), Decal (23), Debris (11), EventArea (27), GUI (16). Default works as a fallback for Terrain; prefer Terrain.

Tags: `Floor` (spawn and pathing surface), `Character`.

## Hard rules

- S01 exactly one Player_Start per scene. S02 under Characters.
- S03 never rotate rooms: Border Left/Right are X, Forward (background) and Back (foreground) are Z.
- S04, S05 Zone_Camera not rotated and under the room's Waypoints.
- S06, S07 at most one SceneRules, in the scene root.
- S08, S09 floors: Tag Floor, Layer Terrain, Static.
- S10, S11 delete the default Main Camera and Directional Light (unused).
- S12 at most 4 active lights (3 on low quality); lights outside rooms are ignored.
- S13 the scene needs an Asset Label or `>> Build` skips it.
- Scenes and prefabs cannot share a bundle; a bundle with no scene is deleted by the builder.
- The Background object is disabled for every room but the one last synced; in game it shows only for the focused room. Keep rooms from seeing each other.
- Doors: one wall collider per door to make intangible; keep door parts off Terrain and Default (use TerrainObstacle).
- Custom armature roots and platform holders at scale (1,1,1) where the doc says so.

## Build output

`>> Build (Compressed) *RECOMMENDED*` or `>> Build (Uncompressed)` writes to `Application.persistentDataPath/SceneObjs/`, which for this project (company Gibbing Tree, product "Madness Project Nexus SceneTools") is `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus SceneTools\SceneObjs\`. Target is StandaloneWindows64.
