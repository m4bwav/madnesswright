# Nexus Prominence (Hank Raid) Playground template

A Playground map template after the fan collab "NEXUS: HANK RAID | MADNESS COMBAT" (hosted by KJ and DUALITY, 2025): Hank storms the Nexus Prominence control tower from the street, room by room. The requester asked for "a template of this map". The video's art belongs to its animators, so this template copies none of it: it rebuilds the route as ten rooms with the game's own proxies and Nexus enemies, in the collab's gray-and-red look.

Status 2026-11-11 (newest): the camera reaches every corner (`FitCamera`; check with `NexusShots.Render -shotDir <dir> -corners`, which logs OK/OUT per corner) and rooms carry Madness dressing (`Gags`). **Run `python tools/make_decals.py <SceneTools>/Assets/Templates/NexusProminence/Tex` before Build**, or the signs and splats are skipped with a warning. Deployed, not played.

Earlier status 2026-11-11: rooms differ in size (`Width`/`Depth` per room in the table; the camera pans between the stretched borders), cover spreads through the floor (`CoverAt`), bigger squads, banners, stripes and floor details; render renders/rooms-2026-11-11c.jpg. Earlier status 2026-11-11: rooms rebuilt to look like the video (shared red-band shell plus a set per room, fog off); render check with `NexusShots.Render -shotDir <dir>`; see renders/. Earlier status: built 2026-11-10 in Unity 2019.2.6f1 (batch mode): the generator compiled first time and made all ten rooms; SYNCH VISUALS, scene-lint (0 errors), the .sobj (1.1 MB), check-mod and deploy all passed. **Not yet played.** Batch route: `Unity.exe -batchmode -quit -projectPath <SceneTools> -executeMethod NexusProminenceBuilder.Build`, then `NexusBatch.SyncAndBuild` (Editor/NexusBatch.cs: SYNCH VISUALS on room 1, save, compressed bundle build).

## Rooms (left to right, each door chains to the next)

| # | Room | From the video | Cover | Enemies (Nexus faction) | Alert |
|---|------|----------------|-------|------------------------|-------|
| 1 | Street checkpoint | city street, "Nexus Prominence City" | barrel, crate | 2 agents | unaware |
| 2 | Wanted hall | wanted posters, floor hatch | wall | 2 agents, 1 scout | unaware |
| 3 | Controls office | "CONTROLS" door, report desk | 2 desks | 2 agents, 1 support | unaware |
| 4 | Cargo bay | red-striped floor, strapped crates | 2 crates, pallet | 2 riot guards, 1 soldat | hunting |
| 5 | Sniper loft | ladder, sniper on a ledge | wall | 2 scouts, 1 agent (sniper points kept) | hunting |
| 6 | Shutter garage | roll-up shutter, vending machine | crate, barrel | 2 engineers, 1 agent | hunting |
| 7 | Spotlight corridor | red ceiling spotlights | none | 3 agents | combat |
| 8 | Locker room | lockers, "employee of the month" | wall | 1 riot guard, 2 agents (locker kept) | combat |
| 9 | Break room | "Agents don't do law", menu board | table, counter | 1 support, 2 agents, 1 scientist | combat |
| 10 | Control tower | monitor wall, server room | 2 desks, counter | 2 soldats, 1 riot guard, 1 engineer | combat |

Player start and the Playground terminal stay in room 1. Weapons are the characters' default loadouts (no card names invented).

## Use it

1. Install Unity 2019.2.6f1 and open the SceneTools project (github.com/MPN-Modders/MPN_SceneTools); see the madnesswright-scenes skill.
2. Copy `Editor/NexusProminenceBuilder.cs` into `<SceneTools>/Assets/Editor/`.
3. Menu **Madness > Templates > Build Nexus Prominence _PG**. It writes `Assets/Scenes/NexusProminence_PG.unity` (asset label `nexusprominence`) and materials under `Assets/Templates/NexusProminence/`.
4. Select `Room_01_StreetCheckpoint`, press SYNCH VISUALS, look through each room, then `mpn.py scene-lint Assets/Scenes/NexusProminence_PG.unity`.
5. Madness > AssetBundle > Build, copy `nexusprominence.sobj` into `NexusProminence/` (the built .sobj is not kept in the repository, for size; the Workshop item 3837705778 carries it), then `mpn.py check-mod NexusProminence` and `mpn.py deploy NexusProminence`.
6. In game: enable the mod, open the Playground terminal, load Nexus Prominence.

## Playtest checklist

- Spawn in the street room; camera borders hold in every room.
- Every chained door leads to the next room and back; the spare doors still spawn.
- Red wall band sits at waist height on the back wall (the generator guesses its height; move it if not).
- Enemies path to every area; each room is clearable.
- `mpn.py log` shows no FAILED or ERROR line for the mod.

## Tweak

Rooms, cover and enemies are the `Rooms` table at the top of the generator; edit and re-run (it asks before overwriting).
