# Nexus Prominence (Hank Raid) Playground template

A Playground map template after the fan collab "NEXUS: HANK RAID | MADNESS COMBAT" (hosted by KJ and DUALITY, 2025): Hank storms the Nexus Prominence control tower from the street, room by room. The requester asked for "a template of this map". The video's art belongs to its animators, so this template copies none of it: it rebuilds the route as ten rooms with the game's own proxies and Nexus enemies, in the collab's gray-and-red look.

Status 2026-11-11 (newest): the camera reaches every corner (`FitCamera`; check with `NexusShots.Render -shotDir <dir> -corners`, which logs OK/OUT per corner) and rooms carry Madness dressing (`Gags`). **Run `python tools/make_decals.py <SceneTools>/Assets/Templates/NexusProminence/Tex` before Build**, or the signs and splats are skipped with a warning. Deployed, not played.

Earlier status 2026-11-11: rooms differ in size (`Width`/`Depth` per room in the table; the camera pans between the stretched borders), cover spreads through the floor (`CoverAt`), bigger squads, banners, stripes and floor details; render renders/rooms-2026-11-11c.jpg. Earlier status 2026-11-11: rooms rebuilt to look like the video (shared red-band shell plus a set per room, fog off); render check with `NexusShots.Render -shotDir <dir>`; see renders/. Earlier status: built 2026-11-10 in Unity 2019.2.6f1 (batch mode): the generator compiled first time and made all ten rooms; SYNCH VISUALS, scene-lint (0 errors), the .sobj (1.1 MB), check-mod and deploy all passed. **Not yet played.** Batch route: `Unity.exe -batchmode -quit -projectPath <SceneTools> -executeMethod NexusProminenceBuilder.Build`, then `NexusBatch.SyncAndBuild` (Editor/NexusBatch.cs: SYNCH VISUALS on room 1, save, compressed bundle build).

## Rooms (left to right, each door chains to the next)

Sets per room are in the generator (`Street`, `WantedHall`, ...); the Workshop description carries the same table with difficulty. Stations and pickups come from `Stations` (fun pass 2026-11-11).

| # | Room | Enemies (Nexus) | Alert | Stations (back wall) | Floor pickup | Difficulty |
|---|------|-----------------|-------|----------------------|--------------|------------|
| 1 | Street checkpoint | 3 agents, rifles | unaware | Ammo | R_NN57 pistol | Easy |
| 2 | Wanted hall | 3 agents, 1 scout | unaware | - | M_SwordNexus | Easy |
| 3 | Controls office | 2 agents, 1 support | unaware | (ammo: no clear wall, skipped) | R_NM46 SMG | Easy |
| 4 | Cargo bay | 2 riot guards, 2 agents, 1 soldat | hunting | Med cabinet, grenade ammo | Shield_RiotLight | Hard |
| 5 | Sniper loft | 2 scouts (battle rifles), 2 agents | hunting | - | R_Hrifle_NFAL | Medium |
| 6 | Shutter garage | 2 engineers, 2 agents | hunting | Ammo | R_NZucker | Medium |
| 7 | Spotlight corridor | 4 agents, 1 riot guard | combat | Med cabinet, ammo | R_Nc762 rifle | Hard |
| 8 | Locker room | 1 riot guard, 2 agents | combat | - | M_SwordNexus | Medium |
| 9 | Break room | 3 agents, 1 support, 1 scientist | combat | Med cabinet | R_NS-V.10 | Hard |
| 10 | Control tower | 2 soldats, 2 riot guards, 1 engineer, 2 agents | combat | Med cabinet, ammo, grenade ammo | R_Hrifle_NFAL | Very hard |

Player start and the Playground terminal stay in room 1. Weapons follow the film (`Loadout` in the generator, the game's own card names): rifles at the street checkpoint, pistols and SMGs in the hall, riot guards with `Shield_RiotLight` and a pistol, soldats with `R_Hrifle_NFAL`, scouts and a few later agents with `M_SwordNexus`; support, engineers and scientists keep their defaults. Wall signs and splats find a clear patch of wall (`CollectWallBusy`) or are dropped with a `no clear wall` log line. The Workshop preview comes from `tools/make_preview.py` over the room shots.

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
