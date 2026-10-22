# Playground Director (was Swap And Unaware)

Take control of any character, freeze the room, set patrols and guards, pose enemies and save scenes, in the story mode, the arenas and the Playground. A requested mod, kept here as the worked example for the [madnesswright-modtools](../../skills/madnesswright-modtools/SKILL.md) skill's Harmony route: an `IModEntrypoint`, two Harmony prefixes, one component per feature, a net48 build against the player's own game files, and the bugs live playtests found.

| Key | What it does |
|---|---|
| F1 | Remove every living character except you, your F8 home body and the playable characters (player controlled, the career's core roster, or a Player-faction card such as Sanford, Deimos or Hank), the way the Playground's Clear All removes them. |
| F2 | Freeze everyone but you (AI, movement, animation). F2 again unfreezes the ones it froze. A frozen character still dies when shot. |
| F3 | Patrol squares (Ancient Warfare 3 style). A black square follows the mouse; left click drops it; U starts another; click a square to select it (yellow outline), then P makes it red (a stop) or black again and Delete removes it. F3 hides and shows them. They belong to the body you control when you place them and only show while you control it. Whenever that body is an NPC, unaware and not frozen or guarding, it walks them in a loop and halts at a red one. While a square is being placed or the mouse is on one, left click does not attack. |
| F4 | On the character under the mouse: a list of idle animations (its own idle emotes, then every looping clip in the game's library, with a filter box). Click one to play it; the character stops wandering while it poses. F4 or ESC closes. |
| F5 | Take control of the character nearest the mouse pointer on screen: a squadmate or an enemy, in any room (through locked arena doors too). Your old body becomes an NPC. Works while you are dead. Lets go of a freeze or guard on that body first. |
| F6 | Every other character goes back to unaware and forgets who it was fighting. They can notice you again. |
| F7 | Keep everyone unaware (re-applied twice a second) until you press F7 again. |
| F8 | Back to your own body (the one you had before the first F5). Happens by itself when the body you took dies, if your own body is still alive. |
| F9 | On the character under the mouse: stands guard. It never moves, goes unaware, and fights anyone not on its team once it sees them (the game's own FixedInPlace spawner option). F9 again: back to normal. |
| F10 | On the character under the mouse: unaware goes to search, search goes to attack (you are its target), attack goes to unaware. |
| F11 | While paused (story, arena, custom stage or Playground): name the scene, ENTER saves it. CAPS LOCK while paused lists saved scenes, this stage's first; click one to load it; ESC closes. See "Scenes" below for what loading does where. |

On the Steam Workshop: https://steamcommunity.com/sharedfiles/filedetails/?id=3822329165 (Subscribe, then enable it from the gear icon).

Needs the `releasebeta` branch: the normal release build does not load mods. Version 0.3.0 was playtested in the Playground on two PCs; 0.4.0 on the owner's PC on 2026-10-21, in the Playground and then the story mode (F9 and F11 not yet exercised), and is on the Workshop. 0.5.0 (scenes in every level) failed its first story playtest (scene files lost the characters); 0.5.1 fixes it and is deployed on the owner's PC, playtest pending, not on the Workshop yet. Back up your saves before using it in a story career.

## Key choices

- The requester asked for F1 to F4 and F9 to F11 and to leave F5 to F8 and F12 alone ("Do not make a button that is not F5, F6, F7, F8, and F12" read as "do not take those"). F12 is Steam's screenshot key.
- No key moved. In a fresh decompile (`decomp.py keys`), F1 to F4, F9 to F11, P and Delete are read only by the developer console (debug builds), the `-devtools` mod-test tools, and the profile window's tab keys while it is open; U and Return are unread in play. CapsLock is the game's autowalk key; the mod reads it only while paused, but the game may still toggle autowalk (any move key turns it off).
- Scenes are files in `AppData/LocalLow/Gibbing Tree/Madness Project Nexus/PlaygroundDirector/Scenes` under the user's profile, outside `Mods/`, because the game reads every `.json` in a mod folder as a MadCard. 0.4.0 scene files still load.

## Scenes (0.5.0)

Why, and what was left out: [the decision](../../ai-docs/decisions/2026-10-21-scenes-in-every-level.md).

- **Playground:** as in 0.4.0. Loading clears the room (the game's Clear All) and spawns the saved characters, each in its saved room if that room is still there; you keep your body and place. A scene saved on another Playground stage loads that stage first.
- **Story levels, arenas, custom stages, hubs:** loading moves you back to the spot and room where you saved, waits a moment for that room's own characters, then puts the characters already there back the way they were (position, facing, damage, alert, frozen, guard, patrol; weapons stay), respawns the ones who have died since, and removes characters the scene does not hold. It never removes your squad, the playable characters, the level's scripted characters (the ones its triggers refer to), bosses or cutscene actors; bosses and cutscene actors are not saved at all.
- Story levels lock each room you leave. If the scene's room is closed, restart the stage (pause, Restart) and load again: from the start, the mod takes you straight to that room.
- A story, arena or custom scene saved on another stage is listed with that stage's name in brackets. Play that stage, then load it: starting a stage writes your career save, so the mod does not do it.
- Removing a character counts as a kill for the level (kill counters, arena kill count). Doors, triggers and objectives are not rewound.
- Arenas cannot be paused between waves (the game blocks it): save and load mid-wave or before the first wave.
- Every save and load writes one line to Player.log (stage, room, counts), so a playtest can be checked from the log.
- Folder, DLL and HarmonyID stay `SwapUnaware`, so the enabled state and the Workshop item carry over; only the display name changed.

## Install

From a zip made by `pack.ps1`:

1. Unzip it anywhere on the computer that plays.
2. Double-click `install.cmd`. It copies the `SwapUnaware` folder into `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus\Mods`.
3. Steam: right-click MADNESS: Project Nexus, Properties, Betas, pick `releasebeta`, and let it update.
4. Launch, click the gear icon at the bottom left of the main menu, select Swap And Unaware, tick ENABLED, Apply, restart the game.

Copying the `SwapUnaware` folder (`info.txt` and `Managed/SwapUnaware.dll`) into that Mods folder by hand does the same.

## Build

Needs the .NET SDK and the game installed. `pwsh ./pack.ps1 -Version 0.4.0` builds `src/` against the game's own `Managed` DLLs (net48, `Private=false`, nothing copied from the game) and writes `dist/SwapUnaware-<version>.zip`. It finds the game with the hub skill's `mpn.py where`; pass `-GameManaged <...\Madness Project Nexus_Data\Managed>` or set `MPN_MANAGED` to point elsewhere. Then deploy with `python -I ../../skills/madnesswright/scripts/mpn.py deploy dist/stage/SwapUnaware --force` and read `mpn.py log` after playing.

To update the Workshop item after a new build: `../WorkshopUpload/out/wsup.exe dist/stage/SwapUnaware preview.jpg "Swap And Unaware" workshop-description.txt "Scripts,Playground" 3822329165` (see [WorkshopUpload](../WorkshopUpload/README.md)).

`build/`, `dist/`, `bin/` and `obj/` stay out of git. Never commit game DLLs or decompiled code.

## How it works

- `ModEntry.Initialize()` adds one component per feature to a host object (`Feature` base class: skips while paused, logs exceptions, switches only itself off after five in a row) plus a one-line on-screen message (`Hud`). Shared helpers (pick under the mouse, playable check, claim release) live in `Game`.
- F2 locks each controller the way cutscenes do (`Controller_Base.LockController`), clears waypoints, pins the rigidbody (`SetFrozen`) and sets the animator speed to 0; all four are undone on F2, on death and when the player takes the body.
- F1 calls `Combatant_Base.RemoveFromGame()` after releasing claims. F10 uses `Controller_Base.SetAlertStatus` and `AI_Manager.SetKillTarget`. F9 copies the spawner's FixedInPlace (`wanderingDisabled`, `SetFrozen(true)`). F4 plays `Combatant_Base.PlayAsEmote(clip, true, false)` with clips from `myAnimSet.IdleEmotes` and `LibraryStorageList.Library.AnimClips`. F3 walks with `AI_Manager.AssignWalkToPosition` and checks arrival by distance. F11 writes each flat record with `JsonUtility` and joins the nesting itself (`JsonUtility` drops the mod's own nested classes), holds its own pause source, closes the game's pause menu, and loads through `InteractiveMode.ClearAll_DoIt()`, `Char_Data.CreateCharacter`, `Combatant_Base.CreateCombatant`, `Squad_Main.AddMember` and `Char_DataAssigner.GiveItemToCharacter`.
- `AttackBlockPatch` skips `Controller_Player.RunControls_Attacks()` while the patrol tool owns the mouse; the game has no "mouse over UI" check of its own.
- F5 picks by screen distance from the mouse over every living NPC, because the game's own `Combat_Manager.ReturnClosestTarget` only accepts targets in your current room. Squadmates go through the game's own `Controller_Player.SwapControllersWithSquadmate(Combatant_Base)` (skipping the core-roster check, which only lives in `CanBePlayerControlled`); anyone else gets `Combatant_Base.AssignController`.
- One Harmony prefix (`LossSwapPatch`) on `Controller_Player.SwapControllersWithSquadmate(bool)`, which the game calls with `overrideChecks: true` when the controlled body dies or leaves the squad: it returns you to your own body, and skips the game's code when the body had no squad.
- F6 releases each NPC's target claim through the game's `AI_TargetClaimer.UnclaimMyTarget` and sets `AI_Wakeup.AlertStatus.Unaware`.

Members checked against beta v2.20.a (build 25615196, 2026-10-18 and 2026-10-21); re-check them in a fresh decompile after a game update.

## What the playtests found

| Version | Bug | Cause in the game's code | Fix |
|---|---|---|---|
| 0.1.0 | Dying in a taken-over Playground spawn threw inside `KillMe` | `SwapControllersWithSquadmate(bool)` reads `myData.mySquad` without a null check; Playground spawns can have no squad | The prefix sends you home, or skips the game's code |
| 0.1.0 | Stuck inside a locked arena | `ReturnClosestTarget` filters by room | Screen-space pick, F8 home |
| 0.2.0 | F5 failed on characters mid-fight (`NullReferenceException` in `AI_TargetClaimer.SetPriority` from `Controller_Base.LostConnection`) | F6 set `myKillTarget = null` by hand, leaving the controller in its target's claimant list; the AI claimed again and `ClaimTarget` does not check for duplicates. `LostConnection` nulls `myOwner`, removes one copy and re-scores the rest, which reads the null owner | Release both controllers' claims before the swap, as `KillMe` does; prune dead and duplicate claimants; F6 unclaims through the game; a swap that still throws reconnects any orphaned controller |
| 0.3.0 | None found; one report of a grunt on your own team turning on you, unconfirmed | Likely the game's Playground spawn: each batch is a squad of `Factions.None`, and a None attacker counts everyone outside its own squad as a foe, you included | Not fixed in the mod: putting a spawn in your squad (the game's `Squad_Main.AddMember`) makes it a real teammate |
| 0.4.0 | None found (owner's PC, 2026-10-21). Player.log showed F1, F2, F3, F4, F10 and F5 to F8 in use, 369 animations listed, no exception; F9 and F11 left no log line and no scene file, so they are not confirmed yet | | |
| 0.5.0 | Story level (Warehouse Raid, 2026-10-22): every load said "saved in room , which the game has closed"; the scene files held no room and no characters. One save threw `IndexOutOfRangeException` in `Health_Manager.ReturnCharacterDead` | `JsonUtility` drops fields typed with the mod's own classes (the mod's assembly loads at runtime), so 0.4.0 and 0.5.0 never wrote characters. `AmDead()` reads `myCorpusTrack[0]`, which is empty on a combatant the game has not set up | 0.5.1: each flat record goes through `JsonUtility` and `SceneData.ToJson`/`FromJson` join and split the nesting; files without a room or characters say "save it again". `Game.Alive` checks the corpus track first |
