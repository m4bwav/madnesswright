---
name: madnesswright-modtools
description: "New art and new code for MADNESS: Project Nexus: the official ModTools Unity project (Unity 2019.2.6f1) turns models, textures, icons, animations and fonts into .mobj AssetBundles with MadObjects (melee, ranged, ranged addons, armor, body parts, Character_Ref rigs, Weapon Animator), wired to MadCards by ObjRef; and Harmony 2.4.2 assemblies loaded from a mod's info.txt (AssemblyPath, HarmonyID, EntryType with IModEntrypoint) with no BepInEx. Decompiles the game's Assembly-CSharp.dll before naming any patch target, and picks the target framework from the game's Managed folder. Use whenever the user asks to 'add a new gun model', 'import my Blender sword', 'make custom armor', 'custom character rig', 'weapon animation', 'write a Harmony patch', 'code mod', 'C# mod for Project Nexus', 'my mobj doesn't load', or names ModTools, MadObj, ObjRef or IModEntrypoint. Plain MadCard edits are madnesswright; levels, maps and .sobj scenes are madnesswright-scenes."
---

# MADNESS: Project Nexus assets and code (ModTools, Harmony)

Outcome: a `.mobj` built by ModTools or a DLL built against the user's own game files, inside a mod that `mpn.py check-mod` passes, with `mpn.py log` output after the user's test. Patch targets come from decompiled game code, never from memory.

`MPN` means `python <madnesswright skill>/scripts/mpn.py` (`../madnesswright/scripts/mpn.py` from here).

## Step 0: freshness (every use, one read)

Read `evergreen.json` next to this file. If `contradiction` is set or today is on or after `next_due`, say so in one line, do the task, then refresh (`evergreen-refresh`) in the same session. If `tests.failing` is non-empty, say so and run `evergreen-tune` after.

## Step 1: is this the right rung?

- Swapping one built-in look for another, or changing stats: a MadCard (`"ObjRef": "*M_Pipewrench"`) in madnesswright. No Unity needed.
- New 3D or 2D content: ModTools (Steps 2 to 4).
- New behaviour no card field exposes: a Harmony assembly (Step 5). Check MCM's glossary first; most tuning is a card value.
- Bosses and other Automatons cannot be modded yet (roadmap, 2026-10).

Setup first, as in madnesswright Step 2: `releasebeta` branch, `MPN backup`.

## Step 2: ModTools project

1. Clone or download `https://github.com/MPN-Modders/MPN_ModTools` (no license file: do not redistribute its files). Install exactly Unity 2019.2.6f1 via Unity Hub (the user installs it) and open the project root.
2. Drop source assets into `Assets` (suggested subfolders exist). Fonts, textures, animations and icons need no extra setup.
3. Models come from Blender or similar; for a model that does not exist yet, hand over to a 3D modelling skill if one is installed (polywright) and come back with an FBX or OBJ.

## Step 3: MadObjects

Details per type, axis rules and the Character_Ref setup: [references/madobjects.md](references/madobjects.md). The essentials:

1. Put the model inside an empty parent GameObject and add the matching `MadObj_` component to the parent (Melee, Ranged, ranged addon, Armor, BodyPart, Character_Ref). Rotate the child, not the parent, so blue Z points forward (the blade's facing or the barrel's firing direction) and green Y is up (toward the tip).
2. Use the component's context menu "Create Points" for the mandatory points (grips, business end, muzzle and casing exits), then place and rotate them. Fit the Box Collider to the object.
3. Drag the parent into the Library to make a prefab. The prefab name is the `ObjRef` value; keep a list of the names.
4. Assign an AssetBundle name at the bottom of the Inspector for every asset to export; same name, same bundle.
5. Build: menu Madness, AssetBundle, `>> Build (Compressed) *RECOMMENDED*` (or Uncompressed). Output: `LocalLow/Gibbing Tree/Madness Project Nexus ModTools/MadObjs/<name>.mobj`; `MPN where` lists the newest under `tool_outputs.modtools`.
6. Copy the `.mobj` into the mod folder with a MadCard whose `ObjRef` names the prefab (not the bundle), for example `{"Type": "Melee", "Filename": "M_ModSword", "Root": "M_SwordLong", "ObjRef": "Machete_Sword"}`. Then `MPN check-mod` and `MPN deploy`.

## Step 4: test

The user enables the mod and spawns the item in the Playground (or equips the armor). Ask them to check the grip, the facing, the muzzle flash and casing position, and the stowed look; then run `MPN log`. Wrong facing is the parent or child rotation; wrong hands are the grip points.

## Step 5: Harmony assemblies

Read [references/harmony.md](references/harmony.md) before writing code. The guards, in order:

1. **Decompile first.** The game is a Mono build: decompile `<install>/Madness Project Nexus_Data/Managed/Assembly-CSharp.dll` from the user's current install (`<install>` is `install.install_dir` from `MPN where`; the Steam folder is named `Madness Project Nexus 2`) with `ilspycmd` (`dotnet tool install -g ilspycmd`, then `ilspycmd -p -o <out> Assembly-CSharp.dll`) or ILSpy or dnSpyEx. Write the decompiled output outside the mod and never publish it. No Harmony target, method name or signature goes into code until it has been found in that output; quote the file and line. If decompiling is not possible, say the patch is a guess.
2. **Confirm the entry interface** in the decompiled code: `IModEntrypoint`, with `Initialize()` and `Unload()`, in the global namespace on build 25615280 (references/harmony.md). If the user's build differs, use what the decompile shows.
3. **Target framework from the game's own files**, not memory: list `Managed/`. If `netstandard.dll` is there, target `netstandard2.0`; otherwise the .NET Framework version of the game's own `0Harmony.dll`, which is `net48` on build 25615280 (no `netstandard.dll` there; `net472` fails to build against that Harmony with MSB3274). Keep the project folder outside the mod folder so `obj/` and its `.json` files never ship (the game reads every `.json` in a mod as cards); the mod gets only `info.txt` and the DLL. Reference `Assembly-CSharp.dll`, the `UnityEngine*.dll` you use and the game's `0Harmony.dll` from `Managed/` with `Private=false`, so none of them is copied into the mod.
4. **Build and wire it.** Put the DLL in the mod (for example `Managed/MyMod.dll`) and add `AssemblyPath`, `HarmonyID` (reverse-domain, unique), `EntryType` (`Namespace.Class`), optionally `PatchOrder` and `ManualPatch` to info.txt. `PatchAll()` runs unless `ManualPatch` is true; `Initialize()` runs either way.
5. **Prove it.** `MPN check-mod`, `MPN deploy`, user enables and plays, then `MPN log`: `mods_log.applied_harmony` must list the HarmonyID; report any `Harmony Patch ... Failed` or `Entrypoint` lines. Log a line from `Initialize()` (UnityEngine `Debug.Log` with the HarmonyID in it) so the load is visible in `Player.log`. Before the user plays, grep the decompile for the callers of every game method the mod calls and guard the ones that cannot handle the state the mod creates (references/harmony.md, "Doing what the game never does"); ask the user to try the edge cases too (the controlled character dying, a room change, leaving and re-entering the mode). An exception's `stack` in `MPN log` names the game method that broke.
   A complete mod built this way, with the bugs its playtests found: [examples/SwapUnaware](../../examples/SwapUnaware/README.md).
6. **After a game update**, re-decompile and re-check every target before telling the user the mod still works.

Legacy routes (replacing `Assembly-CSharp.dll`, BepInEx 5 plugins like the old Not Enough Madness menu) are superseded by built-in Harmony since beta v2.19.a (2026-04-15). Use them only for an old release-branch build, after a backup of the whole `_Data` folder.

## Maintenance

This skill is evergreen: state in `evergreen.json`, research in [RESEARCH.md](RESEARCH.md), edits in [CHANGELOG.md](CHANGELOG.md), lessons in [LEARNINGS.md](LEARNINGS.md), tests in [TESTS.md](TESTS.md). Protocol: [MAINTENANCE.md](MAINTENANCE.md).
