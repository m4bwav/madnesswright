# SceneTools objects, layers and rules

Source: the official SceneTools Overview Google Doc and the MPN_SceneTools repository (TagManager, AssetBundle_Builder_Editor.cs, sample scenes), read 2026-10-17. Ids S01 to S14 are the checks `mpn.py scene-lint` runs.

## Terms

- **Scene**: a Unity scene; a loading screen in the game is a scene change. One `.sobj` can hold many scenes; `.sobj` bundles hold scenes only.
- **Room**: a gameplay area; `Room_Layout` prefab with a RoomObj script, children `Characters`, `Waypoints`, `Background`, `Light_Main`, camera markers.
- **ProxyObject** (`PX_`): stands in for a game object that "does something"; replaced by the real one in game. Type dropdown picks the variant.
- **MarkerObject**: invisible features (pathing, spawns, zones).
- **Custom SceneObject** (`CO_` prefabs): Cover, Togglable, Interactive (+ Locker or Vendor addon), Destructible, Door, Seat built from scratch. A Destructible with bullets passing through, no bar and no explosion makes a cosmetic shootable prop (L-20261111-3 `shootable-set-pieces`). A `CustomAddon_ToggleAutomator` only works on a `CO_` Togglable: on a stock PX_Togglable it stays on the proxy and throws on load; to time stock spikes, drive them from an invisible CO_ Togglable that lists them as Up objects (L-20261111-4 `automator-needs-custom-togglable`).

## Objects

| Object | Use | Rule |
|---|---|---|
| Player_Start | where the player starts; SpawnFromDoor links it to a door | exactly one per scene (S01), under the room's Characters (S02) |
| Char_Start | one placed character by MadCard Filename, with Held, Stow, Thrown, Worn item cards | SpawnEnabled false + a Zone_Spawner to spawn later |
| Waypoint | AI path node | at least one per room; at floor level unless Floating; under Waypoints |
| Sniperpoint | waypoint snipers prefer | |
| Spawnpoint | off-screen spawns; Playground item spawner | place a few floating |
| Item_Spawn | one specific weapon on the floor | |
| PX_Door | doors, manholes, rappel lines; links rooms and scenes | every custom door needs a StartPoint; must be a child of its Room_Layout or it does not work (stated in a community video; unverified in game); S14 warns on a door in the scene root |
| PX_Cover, PX_Locker, PX_Interact, PX_Togglable, PX_Seat | cover, weapon chests, switches and terminals, moving geometry, seats | interactives need a Task to do anything |
| PX_Playground | the Playground control room window | ApplyWindowMask if the wall has no cut-out |
| PX_SiegeRepair | wood for repairing Siege destructibles | Arena |
| Zone_Event, Zone_Dead, Zone_OffLimits, Zone_Spawn(er), Zone_StagingGround | events, kill volumes, teleport-back, triggered spawns, arena squad conscription | |
| Zone_Camera | camera override while the player is inside | never rotated (S04); under the room's Waypoints (S05) |
| Squad_Spawner | waves through doors: Units (Count, GenerationScale), Squad and Max, Weapon Stockpiles, Bosses, Custom Squads | position does not matter per the docs; one community author puts it under Characters ("I believe"; unverified in game). Fields: "Squad_Spawner fields" below |
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
- S14 (warning) a PX_Door in the scene root; doors belong inside a Room_Layout. The lint sees only the direct parent, so a door under some other root object is not caught.
- Scenes and prefabs cannot share a bundle; a bundle with no scene is deleted by the builder.
- The Background object is disabled for every room but the one last synced; in game it shows only for the focused room. Keep rooms from seeing each other.
- Doors: one wall collider per door to make intangible; keep door parts off Terrain and Default (use TerrainObstacle).
- Custom armature roots and platform holders at scale (1,1,1) where the doc says so.

## Spawns, doors and moving parts (official docs, harvested 2026-11-11)

- Weapon stockpiles deal from the end of the list: the last units spawned are armed first, each taking its preferred weapon type; leftovers spawn unarmed. Steer it with list order, RandomSeed and empty Unarmed loadouts, and check in game.
- Doors have four states (Open, Closed, Opening, Closing); nobody spawns through an open or moving door, so spawn doors stay Closed or set NoClosingOrOpening. Custom animated doors run on the ToggleMe bool (template Door_Standard).
- Leave Leeway Mouse at its default in most rooms: mouse-driven pan makes aiming harder.
- A moving CO_ Togglable that characters stand on needs the Platform Addon, or they are flung off or fall through.
- Cutscenes set every ViewCam animator Off at start and fire ToggleMe once per Step: custom camera animators need an Off state.
- Room_Layout keeps rotation 0,0,0 and scale 1,1,1; position is free. A scaled room or a mesh imported at the wrong scale breaks most effects: fix the size with the FBX's Model > Scale Factor, never the Transform.
- Duplicating a room leaves two lights on: press SYNCH VISUALS on one Room_Layout to reset it.
- LinkedDoor is one-way (A to B does not give B to A), so set it on both doors. Story rules disable a room once you leave it; turn on the room's lockout-disable values (`DisableLockout...` on leaving, or on the next room for the previous one) when the player must walk back.
- Every now and then select everything except the Room_Layouts: anything floating outside a room was parented to the wrong room.
- Materials may use custom CG shaders (they are not compiled); C# scripts added in SceneTools never run in the game.
- Mods need the load screen: it converts markers and proxies, so scenes cannot be joined without one.
- Source and unread leads: [../../../ai-docs/notes/2026-11-11-level-technique-harvest.md](../../../ai-docs/notes/2026-11-11-level-technique-harvest.md).

## Squad_Spawner fields (community video _xbtGGh6Rso, harvested 2026-11-12)

- Wave Spawn Type "Mission Waves" takes a Start text, a subtext and an End text shown on screen (e.g. "You won"). Observed in the video.
- Typing "Manual character" in a Units entry fills in a manual character; a Boss entry takes a typed weapon name. Observed.
- Spawn doors are assigned by dragging each door into the spawner's door list. Observed.
- A named Weapon Stockpile preset (e.g. Melee Tier 2, said to hold 17 weapons) caps its entry at that weapon count; to arm more units, add another entry. The author's "from what I understand": unverified in game.

## Hiding the void (same video)

- **The bib**: copies of the back-wall quad placed just under the front floor edge, above the walls and down the sides, with a Room_Cutaway material, so the Dev_Cam never sees the skybox or void. Observed in the video. The top bib can cast a shadow into the room; turning off its shadow casting (Mesh Renderer, Cast Shadows Off) is an inferred fix, unverified in game.
- **The fourth wall**: a copy of the back-wall quad turned 180 degrees with its Mesh Renderer removed, so it blocks but is not drawn. Observed.
- Source: [../../../ai-docs/notes/2026-11-12-madness-technique-harvest.md](../../../ai-docs/notes/2026-11-12-madness-technique-harvest.md), items 23 to 26.

## Beta SceneTools additions (2026-11-12e harvest, items 61-62, 64-65, 67-68, 70-71; beta-only, unverified in game)

From MPN_SceneTools_Testing commit messages only (testing repo, beta branch). Field and prefab names need confirming in the prefabs and tooltips before use.

- Water and fog (2025-12-04): `Hazard_WaterZone` adds water; `GlobalFog` on a room's `Camera_Point` sets that room's fog, and [Sync Visuals] copies it to the Dev_Cam on room sync.
- Lockers on Destructibles: a Locker addon on a Destructible drops its items when destroyed.
- Vessels (2026-01): `CollisionDamageMult`, `ImpactThreshold`, ignore collision with the linked seat user; a DriverAction's `AdditiveForce` adds to velocity instead of setting it, so several can drive one Rigidbody. VesselAnimationEvents exist.
- Seats: samples `Proxy_Seat` and `CO_Seat`; reticle can stay on while seated (turrets); `FaceTowards` takes a limit.
- `Marker_Character` can carry Uniforms (outfit per placed character; fields unknown, 2026-09-02).
- `Effect_ExplosionGenerator`: scripted explosions without a weapon.
- Doors: custom-door `SpawnPoints` was renamed `StartPoints` (old guides use the old name); addon effects, access lights, and a 2026-07-06 lockdown update (details unknown). Stage Start Markers since 2025-06-06.
- Camera Zones have tooltips. The 2026-07-11 "ARENA TEST SETUP" is marked unfinished by its author: custom Arena work on beta may change.

Earlier objects from the same log (2026-11-12f harvest, item 78; 2025 commits, single source, likely in the released tools now, unverified in game): `Hazard_Splosive`, `Marker_Boss` (2025-04-21); AutoShooter, Hazards, GetOut-Zones, Debris Generators (2025-04-18); `AnimEvents_Sound`, `CustomObj_Destructible` (sample `CO_Destructible`), `Obj_Debris`, `Obj_Disposable`, `Obj_Toy`, `Effect_MaterialScroller`, `Effect_RendererVanisher` (2025-04-04); `Marker_Dummy`, scenery characters driven by your own Mecanim controller (2025-02-13); Glow Effect ("80s Lights"); scene Sounds (only loops play on the first frame); Platforms and Automators for Custom Togglables; PatrolParameters and Paths on character spawners; Locker and Vendor addons on Custom Interactives (they take a SerialNumber); Removers (the author had not tested them); Cover Points; `AnimEvent_Interact` for custom interactive animators; Tasks on Lockers; an empty Door type. Events drive many of these by SerialNumber: [event-actions.md](event-actions.md).

Per-component field names (Destructibles, rooms, blockers, zones, vessels, seats, staging grounds, squads, character and boss markers, Removers, hazards, effects, lockers, vendors) from the test repo tooltips: [scenetools-fields.md](scenetools-fields.md) (round g, unverified in game).

## Build output

`>> Build (Compressed) *RECOMMENDED*` or `>> Build (Uncompressed)` writes to `Application.persistentDataPath/SceneObjs/`, which for this project (company Gibbing Tree, product "Madness Project Nexus SceneTools") is `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus SceneTools\SceneObjs\`. Target is StandaloneWindows64.
