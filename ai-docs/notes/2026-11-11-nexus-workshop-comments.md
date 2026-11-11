# Nexus Prominence: Workshop comment review (2026-11-11)

14 comments on item 3837705778 read on 2026-11-11. Insults and anti-AI remarks set aside; two commenters (one a room builder) gave concrete technical faults. Each was checked against `examples/NexusProminence/Editor/NexusProminenceBuilder.cs`.

## Faults, checked

| # | Claim | Check | Fix idea |
|---|---|---|---|
| 1 | No Madness shaders, so nothing has the game's outlines and props look out of place next to vanilla cover | True: every material is `Shader.Find("Standard")` (builder ~line 242, 945) | Use `Madness/Terrain - General` for walls and props, `Madness/Terrain - Ground` for floors (both in SceneTools `Assets/Code/Shaders`); copy settings from a sample room's materials. Biggest visual win. |
| 2 | Props have no collision; cover that does not cover | True for set dressing: `CreatePrimitive` then `DestroyImmediate(Collider)` (~919, 960, 1187) | Keep a BoxCollider on solid props (counters, cabinets, vending machines, crates) on layer Obstacle (13); decals and thin trim stay colliderless. Any freestanding prop meant as cover must use a cover proxy or have collision. |
| 3 | Cover placed in front of a door | Plausible (CoverAt spreads cover in depth) | Add door keep-out boxes to cover placement, like `CollectWallBusy` does for decals. |
| 4 | Doors on fake doors | Plausible: decorative doors (emblem door, doorways) sit beside real PX_Door exits | Drop decorative doors, or keep them only where no real door is within a door-width. |
| 5 | Lights that do not light | Lamp cones are additive meshes; real Lights are capped at 4 per room | Put the 4 real lights under the visible fixtures; remove fixtures with no light, or dim them. |
| 6 | Outdoor area protrudes in more than it should | Room 1 window/skyline and sky strips | Keep outdoor pieces strictly behind the back wall plane. |
| 7 | Fourth wall is 6 uneven triangulated planes; everything is a cube; meshes triangulated | Fourth wall is invisible so cost is minor; cubes are the primitive route | One TerrainInvis box for the camera-side wall. Longer term: bevelled or modelled props (polywright) for hero pieces only. |
| 8 | "90% of the description doesn't happen" | Description lists features some of which were never confirmed in play (spikes timer, vent/rope spawns) | Trim the description to what a play has confirmed; mark the rest as planned. |
| 9 | Rooms are one giant mesh with excess faces | Rooms reuse the sample's `room_pg_room2_walls` mesh | Low priority; build walls from boxes per wall instead. |

Also raised: basing the map on a community film. The Workshop page credits and links the film; keep that.

## Suggested order

1, 2, 3, 8 first (one rebuild, then the owner plays). Then 4, 5, 6. 7 and 9 when time allows.

## For the madnesswright-scenes skill (candidate learnings)

- Generated geometry must use the game's `Madness/Terrain - *` shaders, never `Standard`; otherwise no outlines and it clashes with vanilla cover.
- Props that look solid must collide (Obstacle layer); a scene-lint check could flag MeshRenderers with no collider larger than ~0.5 m in a room.
