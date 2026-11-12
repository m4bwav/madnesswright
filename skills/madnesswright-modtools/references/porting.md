# Porting models: materials, armor, AltParts, melee points, icons

Source: community Steam guides G1 "Modder's Guide to Madness [UPDATED]" (2664043208) and G2 "The Definitive M:PN Modding Guide (INCOMPLETE)" (3305980802), read 2026-11-12 for [the technique harvest](../../../ai-docs/notes/2026-11-12b-madness-technique-harvest.md) (items 32-33, 35-40). Community steps, **unverified in game** unless a line says otherwise. Basic Blender export settings are in [madobjects.md](madobjects.md), "From Blender"; animation clips in [animation-events.md](animation-events.md).

## Effect map colour codes (item 32; community, two guides agree on the scheme)

The effect map is a copy of the texture painted in key colours, and it must be in the AssetBundle too. G1's channel meaning: R = glow, G = shine, B = which tint (126 = untinted), alpha = transparency on glass. G2's palette:

| Colour | Effect |
|---|---|
| black | tint 1 |
| red | tint 1 glowing |
| green | tint 1 shiny |
| blue | tint 2 |
| magenta | tint 2 glowing |
| cyan | tint 2 shiny |
| navy `#00007B` | untinted |
| `#FF007B` | untinted glowing |
| `#00FF7B` | untinted shiny |
| white | cubemap reflection (Standard +Reflect shader only) |

**Conflict, unresolved:** G2's RGB and hex values disagree for magenta and pink (`#FF007B` reads as pink). Paint one swatch sheet and check in game which is right before telling a user. The shaders render bright: darken the base texture to about two thirds.

## FBX import scale (item 33; community, G1 and G2 agree)

| Exported from | Unity import |
|---|---|
| Blender 2.79 | Scale Factor 45, Convert Units ON |
| Blender 2.8+ | Scale Factor 0.45, Convert Units OFF |

So the doc's "0.45" is the importer's Scale Factor (see animation-events.md). Still unverified in game; a quick check of the pair is owed.

## Armor (items 33, 37; community, G2)

1. In Blender 2.79 select only the armor, parent it to the rig with Ctrl+P "With Empty Groups", weight-paint it.
2. Export armor and armature together: FBX 6.1 ASCII, Selected Objects.
3. Unity: Scale Factor 45, untick Import Animations.

Bones to weight to: Ribs/Abdomen/Chest (outfits, backpacks, mask parts that join an outfit), Arm_Upper/Lower (arm parts), Hands/Fingers (claws, gloves), Head (masks, helmets, goggles), Jaw (only head parts with a moving jaw, as on Zed and Abomination bites), Leg_Upper/Lower (outfits), Foot (boots). Never weight to Root or SyncNodeA.

## Inverted normals (item 36; community, G1 and G2)

Fix in Blender: Face Orientation overlay on, select the red faces, Mesh > Normals > Flip, then Shade Smooth (armor, melee and ranged). Left unfixed, the Character - Standard shader's outline ends up inside the mesh; the only other workaround is editing the shader's Outline Width range to allow negative values. Prefer the flip.

## AltParts and alternate materials (item 38; community, G1 and G2)

- An AltPart (a mirror body part) is built like armor, with the Body Part component in place of Armor. Unpack the prefab and move the SkinnedMeshRenderer onto the empty that holds the component.
- Offer them on a character with `"AltParts": ["Prefab1", "Prefab2"]` in the Character card's raw .json.
- Alternate textures: in the Armor component set Material Alts to the number of styles plus the base, one material slot per mesh. G1: the first mesh and material must also be the first alt entry, or the alts cannot be picked in game.

## Melee points (item 35; community, G2)

Right-click the component > Create Points makes `Grip_Main`, `Grip_Off` and `MPoint` (where damage comes from). The Box Collider covers the whole weapon; the material uses the "Items - Weapon" shader; extract the materials from the FBX.

## Origin icons (item 39; community, G1)

Import the icon as a Sprite with transparency, add the texture itself (not an empty holding it) to the AssetBundle, and put its asset name in the Origin card's .json.

## Reading the game's own assets (item 40)

AssetStudio 2.2 (github.com/Perfare/AssetStudio; the user installs it). Open a level file from the game folder, not a bundle, so linked bundles load; `level58` (the Arena hub) has the most preloaded. Model > Export selected objects (split) gives OBJ and PNG. Method: community (G1). Permission: G1 reports the developers allow reuse elsewhere if you credit the dev team and link the Steam page; **single source**, not checked against an official statement. Keep the credit line and use the assets for reference.
