# Textures and modelled props in a SceneTools map

Measured in the SceneTools project (Unity 2019.2.6f1, batch mode) on 2026-11-14 for Nexus Prominence's ten improvement rounds, round 3 (build zm). Editor-side evidence only; how it looks in game is **unverified in game**.

## Tiling textures on `Madness/Terrain - General`

- The shader reads its colour from `_MainTex`. A tiling texture works when the texture's importer has Wrap Mode Repeat and mipmaps on; keep the texture at 512 px for walls and floors so the bundle stays small (four 512 px tiles added about 0.16 MB to a 2.1 MB `.sobj`).
- Unity primitives (Cube) carry 0..1 UVs on every face, so one texture stretches over a 20 m wall and a 1 m crate alike. Give each mesh UVs in metres instead: per vertex, take the two axes its normal does not point along, multiply by the object's scale and divide by the tile size in metres; cache the result per (mesh, rounded scale) as a `.asset` so the bundle carries it. Example: `TileUVs` in m4bwav/nexus-prominence `Editor/NexusProminenceBuilder.cs`.
- Keep the map's look: tint the drawn detail to the material's existing flat colour (luminance around its mean, contrast squeezed) rather than using the picture's own colours, so the outline shader and palette stay. Example: `tools/make_textures.py` in nexus-prominence.

## Making the tile (comfyui-gen, polywright)

- Prompt a flat cartoon texture, straight-on or top-down, "thin black ink outlines on every panel seam", "fills the whole frame edge to edge". Z-Image Turbo (Apache-2.0) gave clean 3 x 3 panel grids first try. Keep the recipe and licence beside the full-size source, outside the repository.
- polywright `pw.py texture --seamless` blends offset copies; on a grid pattern (panels, slabs, tiles) that ghosts a second, shifted grid. Crop the render to whole cells between two seams instead: the crop tiles exactly. Find the seams as dark rows and columns of the grayscale mean.

## Modelled props: import scale

- An OBJ written in metres (Y up) imported into the SceneTools project at **1:1**: a 1.0 x 1.2 x 1.0 m crate measured 1.000 x 1.200 x 1.000 in `Mesh.bounds` after the default import (logged by the builder). OBJ needs no Scale Factor; FBX from Blender has its own rules ([porting.md](../../madnesswright-modtools/references/porting.md), community, unverified).
- Unity mirrors X on OBJ import; a symmetric prop does not care, an asymmetric one should be checked in a render.
- Swap a box prop's mesh and keep its `BoxCollider`, resized to the mesh bounds, so the prop still blocks at the same size and the overlap checks still hold.
- Prove one prop's scale before making more (the build log line is the proof); a polywright code-route mesh is the cheapest first prop.

## Matching a film frame's framing

The game camera is high and three-quarter; a film is often low and close. To compare framing, render each room from a per-room camera fitted by hand to the frame (position across the floor, distance from the back wall, height, pitch, yaw, field of view), and lay film and render over each other at 50% so mismatches show as doubled edges. polywright `pw.py match` places a model into a still in Blender; it does not solve a camera for a Unity room. Example: `NexusShots.FilmCam` in nexus-prominence.
