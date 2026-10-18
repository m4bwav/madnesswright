# Troubleshooting a mod that does not show or does not work

Work down the list; stop at the first fix. Run `mpn.py where`, `check-mod` and `log` first so you are reading facts, not guesses. The loader behaviour below was read from the game's own code (release build 25615280, which carries the beta 2.2 loader) on 2026-10-17.

1. **Wrong build.** `log` shows `game_version.mods_allowed: false` (Player.log says `GAME INITIALIZED: Release v...`): the release build skips the mod loader entirely, so nothing in `Mods/` is read and no `mods.log` is written. Back up, switch to `releasebeta`, relaunch. The `-nomods` launch option also turns the loader off.
2. **Not in the Mods list.** The folder is not directly inside `Mods/` (one level only), there is no `info.txt` or `info.json`, it does not parse (Player.log: `info.txt fail in <folder>`), or `Name` or `Description` is empty (Player.log: `info.txt contains no data`; the mod is skipped). `check-mod` reports the info problems; check the folder depth yourself (`where` lists the folders directly inside `Mods/`).
3. **In the list but not enabled.** Gear icon, select, ENABLED, Apply. `Mods/enabled.txt` (`{"Enabled": [folder names]}`) is the saved list: once it exists, a newly added folder starts disabled. `log` names folders missing from it. Restart after changing files.
4. **Enabled, nothing changes.** The card's `Filename` or `Root` is misspelt (a new, unused entity is created instead of an override; a Root the game cannot find gives no inheritance and no error), the card sits in a `.txt` without `Type` or with the wrong case (`character`; Player.log `TYPE ERROR!`), or in a `.json` that is not strict, is a bare card instead of `{"Character": [...]}`, or uses a list name the game does not know. `mods.log` lists `> FAILED: <Filename> (<Type>)` for cards it could not convert. Compare names with MCM or the asset dump.
5. **Model missing or wrong.** `ObjRef` must be the prefab name inside the `.mobj`, not the bundle file name; the bundle must be in an enabled mod; the bundle was built with ModTools for Unity 2019.2.6f1 (a bundle from another Unity version will not load). `mods.log` lists `> ADDED: <name> (...) from Mod: <mod>` for each object it registered, and `! ERROR: MadObj named "<name>" already exists!` for a clash.
6. **Scene missing from the Playground list.** Scene name lacks `_PG`, the `.sobj` was built without an Asset Label, or the scene fails a SceneTools rule (`mpn.py scene-lint`).
7. **Stage dropped or cannot be played.** The game rejects stages with no valid `SceneName` or no `RequiredCharacters` (madnesswright-scenes).
8. **Assembly mod does nothing.** `mods.log` shows `APPLIED HARMONY PATCH: <id>` on success and `! ERROR: Harmony Patch <id> Failed!` on failure; Player.log carries the exception. `AssemblyPath` is relative to the mod folder (`HARMONY FAIL: Path ... not found.`); `EntryType` is the class's full name (`FAIL: Entrypoint "..." Not Found` or `does not implement IModEntrypoint`); an exception in `Initialize()` rolls the patches back (madnesswright-modtools).
9. **Broke after a game update.** Old 1.08-era mods (Not Enough Madness, DLL swaps) are defunct on the Workshop-era beta. Re-verify game files in Steam and use the official route.
10. **Still stuck.** Post the `mods.log` lines for the mod in the MADNESS: Project Nexus Modding Discord or the game's Steam discussions (links in sources.md).

Log locations (Windows), all under `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus\` (seen on 2026-10-17):

- `Player.log` (Unity's log; `Player-prev.log` is the launch before). Its first lines give the engine (`Initialize engine version: 2019.2.6f1`); `GAME INITIALIZED: <Release|Beta> v<version>` gives the build.
- `Mods\mods.log` (the game's mod log, rewritten each time mods are applied; the official doc calls it `mods.txt`), `Mods\directories.log` (which folder each object came from) and `Mods\enabled.txt`.

Noise on a clean launch, not a mod problem: `Shader is not supported on this GPU ... Hidden/Nature/Terrain/Utilities`, `TRANSITION SQUAD FAIL: NO ONE TRANSITIONING`, `Launch Parameters (0 found)` followed by `[FAILED] ...exe`.
