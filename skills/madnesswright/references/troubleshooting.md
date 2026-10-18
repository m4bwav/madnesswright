# Troubleshooting a mod that does not show or does not work

Work down the list; stop at the first fix. Run `mpn.py where`, `check-mod` and `log` first so you are reading facts, not guesses.

1. **Wrong branch.** `where` shows `branch` other than `releasebeta`: official mods do not load on the release branch (2026-10). Back up, switch, relaunch.
2. **Not in the Mods list.** The folder is not directly inside `Mods/` (one level only), or `info.txt` is missing or does not parse. `check-mod` says which.
3. **In the list but not enabled.** Gear icon, select, ENABLED, Apply. Restart after changing files.
4. **Enabled, nothing changes.** The card's `Filename` or `Root` is misspelt (a new, unused entity is created instead of an override), or the card sits in a `.txt` without `Type`, or in a `.json` that is not strict. Compare names with MCM or the asset dump.
5. **Model missing or wrong.** `ObjRef` must be the prefab name inside the `.mobj`, not the bundle file name; the bundle must be in an enabled mod; the bundle was built with ModTools for Unity 2019.2.6f1 (a bundle from another Unity version will not load).
6. **Scene missing from the Playground list.** Scene name lacks `_PG`, the `.sobj` was built without an Asset Label, or the scene fails a SceneTools rule (`mpn.py scene-lint`).
7. **Stage dropped or cannot be played.** The game rejects stages with no valid `SceneName` or no `RequiredCharacters` (madnesswright-scenes).
8. **Assembly mod does nothing.** `AssemblyPath` is relative to the mod folder; `EntryType` is the full `Namespace.Class`; the class implements `IModEntrypoint`; the DLL targets the game's framework (madnesswright-modtools).
9. **Broke after a game update.** Old 1.08-era mods (Not Enough Madness, DLL swaps) are defunct on the Workshop-era beta. Re-verify game files in Steam and use the official route.
10. **Still stuck.** Post the `mods.txt` lines for the mod in the MADNESS: Project Nexus Modding Discord or the game's Steam discussions (links in sources.md).

Log locations (Windows): `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus\Mods\mods.txt` (official mod log) and, by Unity convention, `...\Madness Project Nexus\Player.log` (unconfirmed for this game until seen; `mpn.py where` reports whether it exists).
