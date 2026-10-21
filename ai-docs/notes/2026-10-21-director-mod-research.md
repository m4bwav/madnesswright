# Game hooks for the Playground Director keys (beta v2.20.a)

Date: 2026-10-21. Decompile of `Assembly-CSharp.dll`, beta v2.20.a, build 25615196, written to the session scratchpad only. Members are named, never pasted. Re-check after a game update: `python -I skills/madnesswright-modtools/scripts/decomp.py find <decompile> <Name>`.

## Keys the game reads

`decomp.py keys <decompile> --keys F1,F2,F3,F4,F9,F10,F11,U,P,Delete,CapsLock,Return`:

- F1 to F4, F9 to F11, P, Delete: read in `Dev_Control` (enabled only when `Game_Manager.IsDebugMode()`, a debug or dev build) and `ModTestTools` (created only with the `-devtools` launch option). Not live in normal play.
- F1, F2, F3 and P: also in `UI_Profile.Update`, which switches tabs only while the profile window is open. In play only `UI_Options.key_Menu_Inventory` (I) opens that window. P is `UI_Options.key_Menu_Stats` but nothing reads it during play.
- CapsLock: `UI_Options.key_Autowalk`, read by `Controller_Player` every frame (toggles autowalk; any move key turns it off again). The only real clash. The mod reads CapsLock only while paused, so it stays as asked.
- Return: character creator and keybind panel only. U: unread.
- Pause: `Game_Manager.ReturnPaused()`, `PauseGame(MonoBehaviour)`, `UnpauseGame(MonoBehaviour)` keep a set of pause sources. `UI_Menu.CreatePauseMenu()` and `UI_Menu.ClosePauseMenu()`; the menu pauses with itself as the source and unpauses on close. `Combatant_Base.Update` does nothing while paused.

## Characters

- Ticking: `Combatant_Base.Update` runs the `myUpdateFunction` delegate, which runs `myController.RunController()`. `Controller_Base` is a plain class, not a component. `KillMe` swaps the delegates; a mod must never swap them.
- Freeze (reversible): `Controller_Base.LockController(Object source)` / `ReleaseController(Object source)` / `AmLocked()` (what cutscenes use), `Pathfind_Main.ClearWaypointsAndTarget(Combatant_Base)` (waypoints move a body even while locked), `Mobile_Object.SetFrozen(bool)` / `AmFrozen()` (rigidbody FreezeAll), animator `Target_Object.myAnimator.myAnim.speed`.
- Kill and remove: `Combatant_Base.KillMe(...)`, `Combatant_Base.RemoveFromGame()` (silent, no corpse; the Playground's Clear All uses it on `Cam_Main.currentRoom.myOccupants`), `InteractiveMode.ClearAll_DoIt()` (static: clears everyone but `Controller_Base.PlayerOne`, loose weapons and decals). `Combatant_Base.incapForced` controls being downed instead of dying. Killing wave units advances arena waves (`Wave_Manager.CharacterKilled`).
- Playable roster: no flag. Use `Target_Object.amPlayer`, `Char_Data.ReturnAmPlayerCharacter()`, `Game_Manager.currentCareer.chars_Core`, and the card's `StatCard_Character.DefaultFaction == Factions.Player` (what the Playground's Change Characters menu marks as a player character).
- Teams: `Char_Data.myTeam`, runtime `Target_Object.myFaction`, `Squad_Main.squadFaction`. `Squad_Main.AddMember(Char_Data)` moves squad and faction together; `Target_Object.ChangeFaction(Factions)` unclaims targets. `Combat_Manager.FactionIsAttackable`: a `Factions.None` attacker treats everyone outside its squad as a foe.
- **Why a Playground grunt "on your team" can turn on you:** `InteractiveMode.UnitSpawn_Done()` puts each spawn batch in `Squad_Main.CreateSquad(Factions.None)` and sets Combat. A None-faction grunt attacks anyone outside its own squad, the player included. Fix: add it to the player's squad with `AddMember`, never by writing `myTeam`.

## AI state

- `AI_Wakeup.AlertStatus`: Unaware, Hunting (search), Combat, Match (spawner placeholder). `Controller_Base.SetAlertStatus(C, status)` adds that state's module set (`Controller_NPC.SetUnawareModules`, `SetHuntingModules`, `SetCombatModules`); modules for other states remove themselves.
- Hunting walks to random spawn points in the room and gives up after about 600 frames. Combat with no target pursues `targetLastLocation`, then falls to Hunting.
- Attack a target: `AI_Manager.SetKillTarget(Controller_Base, Target_Object)` (unclaims, claims through `AI_TargetClaimer`; no faction check), then Combat. Unaware: `AI_TargetClaimer.UnclaimMyTarget`, then `SetAlertStatus(Unaware)` (L-002).
- Guard: the spawner's `Char_DataAssigner.FixedInPlace` sets `Combatant_Base.wanderingDisabled` and `SetFrozen(true)`: the NPC still detects, aims and shoots but cannot move. The mod's F9 does the same.

## Idle animations (F4)

- Played as emotes: `Combatant_Base.PlayAsEmote(AnimationClip, bool fadeIntoEmote, bool checkInValidState)`, ended with `AttemptEndEmote()`, `InLoopingEmote()`. Walking, a new path or a wakeup cancels them.
- Sources: level markers name a clip (`SpawnChatter_Action`), random idles from `Combatant_Base.myAnimSet.IdleEmotes.myClips` (`StatCard_AnimSet`), seat and vehicle loops. Every named clip: `LibraryStorageList.Library.AnimClips.Storage`. Animation cards: `StatCard_Base.ReturnAllCardsByType<StatCard_Animation>()`.

## Moving to a point (F3 patrols)

- Custom node graph per room (no NavMesh). `AI_Manager.AssignWalkToPosition(Controller_Base, Vector3)` paths within the room, or to the door for another room. Arrival: `Target_Object.myWaypoints` empty plus a distance check. `Combatant_Base.wanderingDisabled` stops idle wandering.
- The order system (`Orders_Manager.IssueOrder`, `PatrolPath`, `InterpretOrdersFromPatrolPath`) handles cross-room routes but has a 12-argument signature; not used yet.
- Mouse on the floor: `Target_Object.aim_Intended` of the player's body (from `Cam_Main.UpdateTargetReticlePoint`; not updated while the controller is locked).
- Left click attacks in `Controller_Player.RunControls_Attacks()` (protected). The game has no "mouse over UI" guard; the mod skips that method with a Harmony prefix while a mod tool owns the mouse.

## Scenes (F11)

- No Playground save exists (`Scene_Saver` is empty; Playground careers are never saved and `Saves_SteamCloud.DeleteBadSaves` removes them).
- Playground check: `Game_Manager.currentGameMode == GameModes.Playground`.
- Spawn: `Char_Data.CreateCharacter(StatCard_Character)`, `StatCard_Base.ReturnCardByName<T>(string)`, `Squad_Main.CreateSquad(Factions)`, `Combatant_Base.CreateCombatant(Char_Data, Room_Main, ControllerType, Vector3, Quaternion, bool)`, `Squad_Main.AddMember`, `Wave_Generator.AssignAllDefaultWeapons`.
- Weapons: `Char_Data.itemSlots[slot][hand]`, `itemThrown`; id is the card name (`myCard.name`). Give: `Catalog_Weapons.GenerateWeapon(StatCard_Weapon)` then `Char_DataAssigner.GiveItemToCharacter(item, combatant, slot, hand)` or `GiveThrownToCharacter`.
- Damage: `Target_Object.myTrauma` (no setter method).
- Files: the mod writes `PlaygroundDirector/Scenes/<name>.json` under `Application.persistentDataPath`, outside `Mods/` (the loader reads every `.json` and `.txt` in a mod folder as a MadCard) and outside `<SteamID>/Careers`.
