# First beta launch (2026-10-18)

What the owner's install showed after switching Steam to `releasebeta`. The method is the one in [2026-10-17-real-game-loader.md](2026-10-17-real-game-loader.md); this note adds what a real Beta launch wrote.

## The build

- Steam build 25615196 (`BetaKey releasebeta` in the app manifest), Player.log `GAME INITIALIZED` reads Beta v2.20.a. `mpn.py where` and `mpn.py log` reported `status: Beta`, `mods_allowed: true` with no code change.
- Unity 2019.2.6f1 still; `Managed/` unchanged in shape (`0Harmony.dll`, no `netstandard.dll`), so net48 still applies.
- `Game_Manager.currentReleaseStatus` defaults to `Beta` in this build's decompile, and `ModsEnabled` is `currentReleaseStatus == Beta`.

## Why the owner saw no gear icon before

The main-menu mod button carries `Object_Modding_Remover`, which destroys it in `Awake()` when mods are disabled. On the release branch the button does not exist at all, so "find the gear icon" fails silently there. The skill should say: no gear icon means the game is not on the beta (check the version text on the main menu).

## The real mod files (GreenBloodHank, one container card)

- `Mods/enabled.txt`: `{"Enabled":["GreenBloodHank"]}` (written by the game, compact JSON).
- `Mods/mods.log` (19 lines): `=== MOD LOG ===`, `Scanning for Local Mods...`, `  > FOUND: <path>`, `Scanning for Steam Workshop Mods...`, `[ No Workshop Mods Found ]`, `Mods Enabled:`, `  > (1) [LOCAL] <path>`, `Scanning for MadObjects...`, `Scanning for MadCards...`, ` > OVERRIDE: Hank (Character)  >> Priority (10/0) `, `Scanning for FMOD Banks...`, `=== END OF LOG ===`. `mpn.py log` parsed `loaded` correctly and found no problems. It does not yet report the `OVERRIDE:` lines; a card list from them would prove which cards applied.
- `Mods/directories.log`: empty on this launch.

## Swap-and-unaware mod

Built as the madnesswright-modtools skill's first live test; source kept outside this repo. Every member it calls was found in the beta decompile (Controller_Player.PlayerController, SwapControllersWithSquadmate(Combatant_Base), CanBePlayerControlled holding the core-roster gate, Combatant_Base.AssignController, Combat_Manager.ReturnClosestTarget, Controller_Base.SetAlertStatus and myKillTarget). It needs no Harmony patch: Initialize() adds a MonoBehaviour that polls F5 to F7 (no game code reads those keys). net48 build clean first time, `check-mod` clean, deployed.
