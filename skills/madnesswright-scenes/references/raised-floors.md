# Raised floors: a walkable second height

How the game treats a balcony, platform or upper floor in a SceneTools map. **Read from the decompiled game (Assembly-CSharp, 2026-11-16) and the SceneTools project; not built or played yet, so every line is unverified in game.** Neither sample scene (SamplePlayground_PG, SampleSiegeArena) has a walkable second height; the game's `Effect_RendererVanisher` ("hide roofs and upper floors") hints that upper floors are meant to exist. `mpn.py scene-lint` S24 (info) flags a raised Floor with no Interactive ramp or no PathBlock in the scene.

## What the code says

- **Standing** (player and AI share `Combatant_Base`): a contact counts if the collider is tagged `Floor` or on a walkable layer (Terrain, Obstacle, Default, TerrainInvis, TerrainObstacle, Interactive, Fence), and only if the contact normal's y is over 0.7: slopes up to about 45 degrees. Use 30 degrees or less.
- **No step-up**: there is no CharacterController or step logic. A lip of a few centimetres stops a walker, so stairs need a smooth invisible ramp (layer Interactive, no renderer) with no lip at either end. Movement velocity is horizontal; the ramp contact pushes the capsule up. Steep down ramps cause small hops.
- **Falls**: damage starts at landing speed 8 (about a 3.3 m drop). Keep a balcony at 2.5 units or less for a harmless drop; above about 5 units the fall checks and snap ranges start to matter.
- **AI pathing**: no NavMesh. Per-room graph of waypoints (`Path_Node`) linked by line of sight, fully 3D. A link needs a clear 0.3 sphere cast (Interactive and PathBlock both block it) and ground within 2 units below every point of the line; **downward drop links are allowed**, so an open balcony edge without PathBlock lets NPCs walk off. `ForceConnect` on a waypoint bypasses the checks.
- **Waypoint snap**: waypoints snap down 5.5 units onto Terrain or TerrainObstacle only. One on a Terrain balcony snaps to it; one over an Interactive ramp falls through to the floor below. Keep waypoints off the ramp or mark them Floating.

## Safest first build (one room)

1. Balcony slab: Box, layer Terrain, tag Floor, Static, top at y 2.5, at least 3 units deep, stopped at the back wall.
2. Ramp: one straight invisible Box on Interactive, 25-30 degrees, flush at the foot, overlapping the slab top. Visible steps, if any, without a collider.
3. Waypoints at the ramp foot, the ramp head, two or three on the balcony; `ForceConnect` foot and head both ways.
4. A thin invisible PathBlock box (about 1 unit tall) along every open balcony edge except the ramp head. Fence for a railing that stops bodies but not bullets.
5. Invisible ceiling collider with at least 2.5 units of head room over the balcony.
6. Playtest: one Char_Start (Hunting) on the balcony, the player downstairs. Watch the AI use the ramp and the player walk up without sticking.

A drawn slope or steps with a BoxCollider is a solid box: players read it as climbable and bump into it. `mpn.py scene-lint` S25 lists such pieces (`info.solid_slopes`) and warns when the scene has no Interactive ramp. A low deck (under 2 units) needs no PathBlock on its open edge: the drop is within the 2-unit link ground gap, so links over it are ordinary two-way links (built in Nexus Prominence build zzc, a 0.675-unit deck and a 10.6 degree ramp; unplayed).

## Built so far (Nexus Prominence, editor-side, unplayed unless noted)

- Three 0.675-unit floors with Interactive ramps: room 4's back deck (zzc, 10.6 degrees), room 6's platform under two pylons with a ramp down its side (zzj, 14 degrees), room 8's landing whose drawn treads hide a ramp (zzj, 14 degrees). S24 lists them by room.
- A ramp in any direction: lay the box with `Quaternion.LookRotation(down * cos(a) - up * sin(a), down * sin(a) + up * cos(a))`, where `down` is the horizontal way to the foot and `a` the slope angle; size (width - 0.1, 0.3, length + overlap + burial).
- A new floor over ground the template already used: move every waypoint inside its footprint up onto its top (they would sit inside the collider) and every waypoint over the ramp past its foot.
- Editor scripts: never `x = GetComponent<T>() ?? AddComponent<T>()`. Unity's fake null is not C# null, so `??` keeps a missing component and the next access throws MissingComponentException; test with `== null`.

Untested: camera borders with a balcony, cover on a balcony, Squad Spawner entries and spawn floor checks at height. `Leap_Area` (NPC jumps) exists in the game but is not exposed in SceneTools.

Sources: R-20261116-1 in [../RESEARCH.md](../RESEARCH.md); layer meanings in [scenetools-rules.md](scenetools-rules.md).
