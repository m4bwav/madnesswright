# Swap And Unaware (MADNESS: Project Nexus mod)

Take control of any character and make everyone else stop noticing you. A requested mod, kept here as the worked example for the [madnesswright-modtools](../../skills/madnesswright-modtools/SKILL.md) skill's Harmony route: an `IModEntrypoint`, one Harmony prefix, a net48 build against the player's own game files, and the bugs a live playtest found.

| Key | What it does |
|---|---|
| F5 | Take control of the character nearest the mouse pointer on screen: a squadmate or an enemy, in any room (through locked arena doors too). Your old body becomes an NPC. Works while you are dead. |
| F8 | Back to your own body (the one you had before the first F5). Happens by itself when the body you took dies, if your own body is still alive. |
| F6 | Every other character goes back to unaware and forgets who it was fighting. They can notice you again. |
| F7 | Keep everyone unaware (re-applied twice a second) until you press F7 again. |

On the Steam Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=3822329165 (Subscribe, then enable it from the gear icon).

The F keys were picked because no game code reads them (checked in the decompile). Needs the `releasebeta` branch: the normal release build does not load mods. Tested in the Playground; not in the story career, so back up your saves before using it there.

## Install

From a zip made by `pack.ps1`:

1. Unzip it anywhere on the computer that plays.
2. Double-click `install.cmd`. It copies the `SwapUnaware` folder into `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus\Mods`.
3. Steam: right-click MADNESS: Project Nexus, Properties, Betas, pick `releasebeta`, and let it update.
4. Launch, click the gear icon at the bottom left of the main menu, select Swap And Unaware, tick ENABLED, Apply, restart the game.

Copying the `SwapUnaware` folder (`info.txt` and `Managed/SwapUnaware.dll`) into that Mods folder by hand does the same.

## Build

Needs the .NET SDK and the game installed. `pwsh ./pack.ps1 -Version 0.3.0` builds `src/` against the game's own `Managed` DLLs (net48, `Private=false`, nothing copied from the game) and writes `dist/SwapUnaware-<version>.zip`. It finds the game with the hub skill's `mpn.py where`; pass `-GameManaged <...\Madness Project Nexus_Data\Managed>` or set `MPN_MANAGED` to point elsewhere. Then deploy with `python -I ../../skills/madnesswright/scripts/mpn.py deploy dist/stage/SwapUnaware --force` and read `mpn.py log` after playing.

`build/`, `dist/`, `bin/` and `obj/` stay out of git. Never commit game DLLs or decompiled code.

## How it works

- `ModEntry.Initialize()` adds a component that polls the keys. No game code is patched for them.
- F5 picks by screen distance from the mouse over every living NPC, because the game's own `Combat_Manager.ReturnClosestTarget` only accepts targets in your current room. Squadmates go through the game's own `Controller_Player.SwapControllersWithSquadmate(Combatant_Base)` (skipping the core-roster check, which only lives in `CanBePlayerControlled`); anyone else gets `Combatant_Base.AssignController`.
- One Harmony prefix (`LossSwapPatch`) on `Controller_Player.SwapControllersWithSquadmate(bool)`, which the game calls with `overrideChecks: true` when the controlled body dies or leaves the squad: it returns you to your own body, and skips the game's code when the body had no squad.
- F6 releases each NPC's target claim through the game's `AI_TargetClaimer.UnclaimMyTarget` and sets `AI_Wakeup.AlertStatus.Unaware`.

Members checked against beta v2.20.a (build 25615196, 2026-10-18); re-check them in a fresh decompile after a game update.

## What the playtests found

| Version | Bug | Cause in the game's code | Fix |
|---|---|---|---|
| 0.1.0 | Dying in a taken-over Playground spawn threw inside `KillMe` | `SwapControllersWithSquadmate(bool)` reads `myData.mySquad` without a null check; Playground spawns can have no squad | The prefix sends you home, or skips the game's code |
| 0.1.0 | Stuck inside a locked arena | `ReturnClosestTarget` filters by room | Screen-space pick, F8 home |
| 0.2.0 | F5 failed on characters mid-fight (`NullReferenceException` in `AI_TargetClaimer.SetPriority` from `Controller_Base.LostConnection`) | F6 set `myKillTarget = null` by hand, leaving the controller in its target's claimant list; the AI claimed again and `ClaimTarget` does not check for duplicates. `LostConnection` nulls `myOwner`, removes one copy and re-scores the rest, which reads the null owner | Release both controllers' claims before the swap, as `KillMe` does; prune dead and duplicate claimants; F6 unclaims through the game; a swap that still throws reconnects any orphaned controller |
