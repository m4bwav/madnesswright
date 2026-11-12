# Learnings: madnesswright-scenes

Procedural lessons for [SKILL.md](SKILL.md). Research findings live in [RESEARCH.md](RESEARCH.md); every change is logged in [CHANGELOG.md](CHANGELOG.md); test runs in [TESTS.md](TESTS.md); state in `evergreen.json`. Format and write-time gate: MAINTENANCE.md (LEARNINGS-FORMAT). Retired entries go to LEARNINGS-ARCHIVE.md with a reason.

Write an entry the moment a real signal happens: a user correction, the same error twice, a discovered workaround, an environment fact, a stated preference, a failed test or a failure in use. Check existing entries first, by meaning (`evergreen.py search "<the lesson>" --kinds learnings` finds near-duplicates in every registered unit): add / update / retire / none. Trigger and Hypothesis are required. Promote after three confirmations; retire when harmful > helpful.

## Active

### L-001 · 2026-10-17 · A sibling hub that says "start here" eats the specialist's triggers
- Trigger: T-20261017-4, all six scenes trigger runs went to madnesswright or madnesswright-modtools on 0.1.0
- Hypothesis: the hub's broad "mod Project Nexus" and "Start here" outrank a specialist description that opens with tool names; ".sobj" read as close to ".mobj"
- Rule: open the description with the user's words (level, map, stage) and say "requests start here"; give each sibling an explicit sentence routing levels here; re-run every sibling's triggers after any description edit, with a runner that records more than the first Skill call
- Evidence: C-20261017-2, T-20261017-4 (36/36 after), confirmed 2026-10-17
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-17

### L-002 · 2026-10-23 · A rule the brief cannot meet gets broken silently; a limit in the column header holds
- Trigger: T-20261023-1, outcome-2 asked for five enemy types in five waves; 3/3 runs debuted two in the finale without a word; after a prose rule, 1 of 3 still set "Max attacking 3"
- Hypothesis: the model satisfies the brief first and treats prose rules as advice; a column it must fill states the limit at the moment it writes the number
- Rule: give each pacing rule an escape ("say so and offer another wave") and put the number limits into the template's column headers
- Evidence: C-20261023-2, T-20261023-1 round 3 (3/3)
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-23

### L-003 · 2026-11-11 · A generated SceneTools map is unchecked until each room is rendered from its camera
- Trigger: the Nexus Prominence template (2026-11-10) shipped to the Workshop as ten bare sample boxes in red fog; the player said it "looked nothing like the video". Nobody had looked at a room before deploy (proxies only show in game).
- Hypothesis: lint and check-mod prove the map loads, not how it looks; a headless render per room is cheap and shows it
- Rule: before deploy, render every room from its CameraPoint (examples/NexusProminence/Editor/NexusShots.cs, `-executeMethod NexusShots.Render -shotDir <dir>`) and compare with the reference. Traps seen: (1) enable only that room's lights, or every room's directional light washes it white; (2) clone all rooms from the bare template before dressing any, or clones copy room 1's enemies and set; (3) `EditorUtility.DisplayDialog` returns Cancel in `-batchmode` (guard with `Application.isBatchMode`); (4) SamplePlayground's rooms carry FogDensity 0.1 (zero it unless fog is wanted); (5) the bundle build writes to LocalLow/Gibbing Tree/Madness Project Nexus SceneTools/SceneObjs
- Evidence: madnesswright ai-docs/log.md 2026-11-11; renders examples/NexusProminence/renders/rooms-2026-11-11.jpg; build.log "Cancelling DisplayDialog ... should not be called in batch mode"
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-11-11

<!-- Example (delete once you have a real entry):
### L-001 · 2026-10-17 · One-line lesson in plain words
- Trigger: what happened, with dates or counts
- Hypothesis: why
- Rule: the shortest instruction that prevents the trigger
- Evidence: C-20261017-1, T-20261017-1, confirmed 2026-10-17
- Scope: skill | repo:<slug> | env:<name> | global
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-17
-->

## L-20261111-1 `stretch-cloned-rooms`
- Trigger: every room cloned from the sample box looks the same square; a script resizes rooms
- Rule: scale room meshes and colliders about one wall and move everything else (doors, cover, markers, lights, Border_*); the sample meshes are rotated -90 on X, so scale each local axis by the world axis it lines up with; call `Physics.SyncTransforms()` before reading collider bounds. Wide rooms pan, but see L-20261111-2 `camera-reaches-corners`.
- Evidence: examples/NexusProminence `Stretch`, build logs 2026-11-11 (widths stayed 31.5 until both fixes), decompiled Cam_Main
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-11-11

## L-20261111-2 `camera-reaches-corners`
- Trigger: in a wide or deep room the player walks off screen at the front corners, or an exit door stays half hidden
- Rule: Cam_Main follows depth only by the room's Leeway z (0 in the sample room, so never toward the front) and stops `|CameraTarget - CameraPoint| * fov * 0.005 * (aspect - 1)` inside Border_Left/Right once the room is over 17 units wide (about 5 units in the sample). Set Leeway_Pos.z and Leeway_Look.z (0.6 works) and place the side borders so that margin lands a little past a player at the wall; check with a render that reproduces the follow (examples/NexusProminence `FitCamera`, `NexusShots.Render -corners`).
- Evidence: decompiled Cam_Main.UpdateView (build 25615196); corner renders 2026-11-11: 19 of 40 corners off screen before, 0 after
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-11-11

## L-20261111-3 `shootable-set-pieces`
- Trigger: a map should react to gunfire (glass, lamps, screens) without changing the fight
- Rule: wrap the prop in a `CustomObj_Destructible` (the game converts it to `Interactive_Target`) with an Intact state shown until hit and a Broken state after; keep `AllBulletsPassThrough` on, no damage bar, no popup text, no explosion, `FlashColorOnHit` on; use built-in sound names (Wine Bottle Break, Sparks/Electrocution); set hit boxes on the back wall, overhead or inside cases, out of the walk lane. Played clean: sound, flash and swap show, no bar or marker icon, bullets still reach enemies behind.
- Evidence: examples/NexusProminence `Breakable` and juice-changes.md; owner play 2026-11-11 ("play test went well"), mods.log clean; Workshop item 3837705778 updated 14:02
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-11-11

## L-20261111-4 `automator-needs-custom-togglable`
- Trigger: a timed trap or moving platform uses `CustomAddon_ToggleAutomator`, or Player.log shows `NullReferenceException at Toggleable_Automator.SetAutomatorState` from `Toggleable_Controller.Awake`
- Rule: the converter replaces a stock PX_Togglable with a new library object and moves no add-ons, so an automator on it finds no `Toggleable_Main` and throws (the trap never cycles; mods.log stays clean). Put the automator on a `CustomObj_Togglable` (the game adds `Toggleable_Main` to that object); to time stock spikes, give that invisible driver the PX_Togglable in `UpObjs` (the converter swaps the entry for the created object) and leave the stock one StartUp on. Read Player.log, not only mods.log, after every play.
- Evidence: owner play 2026-11-11 14:16 (3 NREs, one per spike strip); decompiled Converter_Proxy.ReplaceProxy/ConvertCustomObj, Toggleable_Controller.Init (build 25615196); examples/NexusProminence `Spikes`
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-11-11

## L-20261111-5 `sample-walls-one-mesh`
- Trigger: a room should look out over a distant view (a window, an open balcony), or a shallower room leaves an empty band at the bottom of the screen
- Rule: the sample room's back and side walls are one mesh (`room_pg_room2_walls`), so to see past the back wall hide that renderer (keep its collider, so nobody walks out) and rebuild the side walls and the wall around the opening as boxes; put the view 5 to 24 units behind the wall (at 40 to 60 it rendered black) and grow distant layers by about 1 + dz/14 so they still fill the window. When a stretch makes a room shallower, also move CameraPoint back in z by about half the removed depth: the camera otherwise stays put and the lower screen shows void.
- Evidence: examples/NexusProminence `OpenBackWall`, `Stretch`; build-p log 2026-11-11 (back ray hit the single walls mesh), owner's in-game shot of the empty lower half; shots-p 40/40 corners
- Scope: skill
- Status: retired · helpful 0 · harmful 1 · last_confirmed 2026-11-11 (owner: hiding the walls mesh, the depth/width scales and the 0.55 zoom broke the rooms and the Playground room; all reverted in build-r. Do not hide the sample walls mesh.)

## L-20261111-6 `madness-shader-solid-props`
- Trigger: generated or primitive geometry in a room; players say props look out of place next to the game's cover, have no outlines, or "cover that doesnt cover"
- Rule: give walls, floors and props the game's `Madness/Terrain - General` shader (it reads colour only from `_MainTex`, so a solid colour needs a small texture; `_Outline` about 0.06), never `Standard`: that is what draws the black Madness outline and lights from the room's ambient. Keep `Standard` only for glowing pieces (the terrain shader has no emission). Any prop that looks solid and stands on the floor needs a collider (Obstacle layer 13), kept clear of doors. Never give the camera-side wall a visible closed mesh: a box there faces the camera and blacks out the view; keep it one-sided or renderer off.
- Evidence: Workshop comments on item 3837705778 (2026-11-11); SceneTools sample materials Room_Cutaway_* use Terrain - General; examples/NexusProminence `Mat`, `Solidify`, `BoxFourthWall`; build-w (box 4th wall: all-black shots) and build-y (outlines visible, 40/40 corners)
- Scope: skill
- Status: active · helpful 0 · harmful 0 · last_confirmed 2026-11-11

## L-20261111-7 `wall-filler-crosses-doors`

- Trigger: generated wall dressing on a fixed rhythm (pilasters, conduits, pipe clamps, stripes, panel seams) in a room whose doors, windows, posters or cases are placed per room; owner's shot: "struts going over the doors"
- Rule: after a room is dressed, run an overlap pass: any filler piece whose bounds cross a feature (door, window, sign, poster, case, banner, locker, crate) or a real door opening is removed, and feature-vs-feature overlaps are logged for a hand fix. Dump the room's boxes to JSON and check them with polywright `pw.py overlaps` so the check works outside Unity too. Props (crates) must test the real door positions before placing.
- Evidence: examples/NexusProminence `ClearWallOverlaps` (build-z removed 62 filler pieces across 10 rooms and logged a painted door inside room 3's window, crates in a room 4 door opening and a slate over a room 7 banner, fixed in build-za); polywright 0.6.4 `overlaps`
- Scope: skill
- Status: active · helpful 0 · harmful 0 · last_confirmed 2026-11-11
