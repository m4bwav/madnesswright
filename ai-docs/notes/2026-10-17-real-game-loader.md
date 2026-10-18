# The real game and its mod loader (2026-10-17)

What an installed copy of MADNESS: Project Nexus showed on 2026-10-17, and how it was read. The skills' references and RESEARCH.md entries (madnesswright R-20261017-9, madnesswright-modtools R-20261017-6) carry the facts; this note is the method, so the next session can repeat it after a game update.

## The install

- Steam build 25615280, public branch, `GAME INITIALIZED: Release v1.11.a` in Player.log, Unity 2019.2.6f1 (fe82a0e88406). The Steam folder is `Madness Project Nexus 2`; the data folder is `%USERPROFILE%/AppData/LocalLow/Gibbing Tree/Madness Project Nexus/` and holds `Player.log`.
- `Madness Project Nexus_Data/Managed/` holds `0Harmony.dll` (Harmony 2.4.2.0, built for net48), `mscorlib.dll`, `Assembly-CSharp.dll`, no `netstandard.dll`.
- A save backup made with `mpn.py backup` held five files, the career file among them.

## How the loader was read

`dotnet tool install -g ilspycmd` (11.1), then `ilspycmd -p -o <scratch> Assembly-CSharp.dll`. The output stayed in the session's scratch folder; none of it is in this repository, and none may ever be. The classes that matter: `Mod_Manager` (scan, enable list, logs, Harmony), `ModItem` and `WorkshopItem` (info keys), `Converter_JSON` (card files), `AssetBundle_Handler` (bundles), `Game_Manager` (the Beta gate, versions), `Saves_Manager` (folder paths), `Steam_WorkshopIntegration`.

## What it showed that the docs do not say

1. The mod log is `Mods/mods.log`; the official Modding Overview calls it `mods.txt`. There is also `Mods/directories.log` and the enabled list `Mods/enabled.txt` (`{"Enabled": [folder names]}`), which disables any folder it does not list.
2. The loader runs only when the build reports `Beta` and `-nomods` is absent. The release build carries the whole loader, with a version table whose beta entry reads 2.2 (v2.20.a).
3. Info is `info.txt` or `info.json`; Name and Description must both be non-empty.
4. `.txt` files are split into top-level `{}` cards after curly double quotes are straightened; `Type` maps to a class name, case-sensitive. `.json` files are read only as a container keyed by type name; bare cards and unknown keys are dropped silently, and quotes are not cleaned.
5. `IModEntrypoint` is in the global namespace. A blank HarmonyID becomes `local.<new guid>` each launch (or `workshop.<id>`). An exception in `Initialize()` unpatches the mod.

## Not yet seen

A `mods.log` written by a real Beta launch. A test mod, `GreenBloodHank` (one container card: Hank, Root Hank, Blood green), sits in the game's Mods folder for that check; it waits for the owner to switch to releasebeta.
