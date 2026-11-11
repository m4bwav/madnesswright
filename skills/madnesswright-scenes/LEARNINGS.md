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
