# Scenes outside the Playground: levels, rooms, saves, scripts

Research for Playground Director 0.5.0 (F11 scenes in story levels, arenas and custom stages). Fresh decompile of beta v2.20.a, build 25615196, on 2026-10-21, kept in the session scratchpad. Member names and behaviour only; re-check each with `decomp.py find` after a game update. The Playground-only findings for 0.4.0 are in [2026-10-21-director-mod-research.md](2026-10-21-director-mod-research.md).

## Which level is this

- `GameModes`: Story, Arena, Custom, Playground, Tutorial, None. `Game_Manager.currentGameMode`.
- The stage is `Stage_Manager.currentStage` (a `StatCard_Stage`: `_CAMPAIGN` > `_Story` / `_Custom`, `_Arena`). Null in hubs reached without a menu; `ReturnStageIsOverworldHub()` is true then or when the card is a hub. Display name: `StatCard_Base.FullName`, then `ShortName`, then the card name.
- There is no level index or mission id. Story progress is per card name in `Career_Data.myStageData`.
- `Game_Manager.GameTransitions.NameOrPathOfCurrentScene()` returns the scene path for a modded (.sobj) scene and the name otherwise, so two custom stages with the same file name stay apart.
- Key used by the mod: `mode | stage card name or "hub" | scene name or path`. Whether several story cards share one Unity scene is not settled (it is asset data); the card name in the key covers that.

## Rooms

- `Room_Main` has no id. `Room_Main.allRooms` (static HashSet, unordered) lists the level's rooms. Identify one by GameObject name plus `World_Object.SerialNumber` (often empty), then nearest position. A combatant's room is `World_Object.myRoom`; the focused room is `Cam_Main.currentRoom`.
- No position-to-room lookup exists: save `myRoom` with each character.
- `RoomBecomesFocus` runs the room's character starters (`DataAssigner.StartMe`): a room's own characters spawn when the player first enters it.
- Leaving a room in Story, Arena and Custom non-hub stages (`ModeFeatures.RoomExitLockout`): `RoomExit` > `DisableRoomOnLeave` > `ClearRoomOfObjects` (removes characters in temporary squads that are not essential), spawners off, `amLockedOut = true`, room GameObject off (unless `keepRoomActiveOnLeave`). So a story level is a chain of rooms you cannot go back to.
- `Combatant_Base.TeleportTo(pos, forward, room)` calls `room.GiveToRoom`; for the focal (player) character that calls `RoomBecomesFocus`, which spawns the new room's characters and locks the old one.
- Room doors lock while more than one combat faction is in the room (`UpdateFactionPresent`); a live count, nothing latches.

## Pause

- One pause path for every mode (`UI_Keystrokes`, `Game_Manager.CanPause()`). Blocked on menu scenes, scenes with `disablePauseMenu`, **between arena waves** (`Arena_Manager.WaveCompleted()`), on the stage end screen, and while a `UI_MenuButtons` menu is open. Cutscenes do not block it.
- `Game_Manager.ClearPauseGame()` wipes every pause source, the mod's too, on every scene load.
- F11 is read only by the debug console; CapsLock only by autowalk. Nothing in story or arena code reads either.
- `Event_Manager.Update` runs while paused (one queued condition per frame); `Wave_Manager.Update` does not.

## What ties a level to its characters

- `RemoveFromGame()` runs `KillMe()` then despawns: every death callback fires (squad death broadcast, `Wave_Manager.CharacterKilled`, `CharacterDies` event conditions, faction presence, possible arena powerup).
- Scripts find characters by **serial number** (`Event_Manager.RegisterSerialNumber`; event actions, cutscene actors, `Event_Area`, `CharacterDies` by serial or card). A combatant made by `CreateCombatant` has none; `World_Object.SetSerialNumber(string)` registers one.
- `ConditionTypes.FactionDead` fires when no squad of a faction has a living member; `TrueForever` conditions latch. Keeping one member alive (respawn before removing) avoids it.
- `CharacterDies` counters add up every removal ("kill N of X").
- Bosses (`Controller_Base.AmType_Boss()`, `Boss_DataAssigner`) and cutscene actors (`Target_Object.myCutscene`) carry links a respawn cannot rebuild.
- Flags to skip: `Target_Object.amEssential` (player squad, designer-marked, arena clones), a serial number ("PSQUAD"+n is the player squad), `Combatant_Base.myWave`, `Squad_Main.AmPlayerSquad`, `squadTemporary`.
- `InteractiveMode.ClearAll_DoIt()` runs anywhere but removes everyone in the current room except PlayerOne (squadmates, bosses, scripted characters) and despawns quest items. Playground only.

## Arenas

- Living wave units are counted by squad rosters in `Wave_Tandem.activeSquads`; a wave ends when they are empty and nothing is left to spawn, then `WaveComplete` kills every remaining enemy and saves the career.
- Removing wave units counts as killing them and can end the wave (and the stage on the last wave).
- A unit the mod spawns is not in the wave unless added: `WaveContainer.AddUnit(combatant)` (used by battle rooms) moves its squad into the tandem, so the wave waits for it.

## Loading a stage and the career save

- `Game_Manager.SaveCareer()` saves only in modes with `SaveProgress` (Story, Arena, Custom). It is called from `Stage_Manager.StageSelectedFromMenu` (every stage start), `LeavingStage`, arena wave start and end, event action `Act_SaveProgress`, hub menus. Not on death, restart or a bare scene load.
- Start a stage by code: `StageSelectedFromMenu(card, difficulty)` then `GameTransitions.LoadScene(card.SceneName, ...)`; this writes the career (squad reset, last played). The mod does not do it.
- Playground stages switch with `GameTransitions.LoadScene(scene, TransitionType.WipeSide)` (what `InteractiveMode.Stage_Load` does); Playground careers are never saved.
- Death in a story stage: the end screen, then Retry reloads the scene from the start (`RestartLevel` + `RetryFromCheckpoint`; mid-level checkpoints are unused). Every reload clears squads, rooms and events, so spawned characters are gone.
- Characters made by `CreateCombatant` in a squad from `Squad_Main.CreateSquad` never reach the career roster lists. Adding one to the player's squad would write its id into `IDs_Squad` (ignored on load, but junk), so the mod gives restored "your squad" characters their own squad outside the Playground.

## Still unsure

- Which scenes set `disablePauseMenu`.
- Whether a room's starters spawn its characters in the same frame as `RoomBecomesFocus` (the mod waits 0.75 s).
- Stale `myOccupants` entries after `ClearAll_DoIt` destroys corpses in other rooms.
