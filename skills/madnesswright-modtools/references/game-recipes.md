# Game recipes for Harmony and behaviour mods

Members of MADNESS: Project Nexus that mods change characters with, found in a decompile of beta v2.20.a (build 25615196) on 2026-10-21 for the Playground Director mod ([examples/SwapUnaware](../../../examples/SwapUnaware/README.md)). Names only. Re-check each with `decomp.py find <decompile> <Name>` after a game update. Status: built and deployed; **not playtested yet** unless a line says so.

## Look things up without reading whole files

`python scripts/decomp.py` (beside this skill) prints one JSON object per command:

- `decompile <Assembly-CSharp.dll> --out <scratch dir>`: runs ilspycmd. Keep the output in a scratch folder, never in a repo.
- `keys <dir> --keys F1,U,Delete`: every key the game reads, rebindable defaults (`UI_Options.key_*`), and which files only run in debug builds or with `-devtools`. `clashes` lists keys read in normal code; open each file before moving a key: a menu that reads a key only while it is open (the profile window's F1 to F3 and P) is no clash in play.
- `find <dir> <Name>`: declaration lines (file, line, signature). `callers <dir> <Name>`: call sites, for L-001's caller check.

## Keys and pause

- Poll keys from a `MonoBehaviour` the mod adds; `Combatant_Base.Update` returns early while `Game_Manager.ReturnPaused()`, so skip gameplay keys then too.
- Pause has sources: `Game_Manager.PauseGame(MonoBehaviour)` and `UnpauseGame(MonoBehaviour)`. A mod window that needs ENTER and clicks pauses with itself as a source, then `UI_Menu.ClosePauseMenu()` (the menu unpauses only its own source).
- CapsLock is autowalk (`UI_Options.key_Autowalk`), read every frame by `Controller_Player`.

## Characters

- Freeze and unfreeze: `Controller_Base.LockController(Object)` / `ReleaseController(Object)` (the cutscene lock), `Pathfind_Main.ClearWaypointsAndTarget(Combatant_Base)` (waypoints move a locked body), `SetFrozen(bool)` (rigidbody), `myAnimator.myAnim.speed`. Undo all four on death and before a controller swap: the lock is on the controller, not the body.
- Never swap `myUpdateFunction` and the other update delegates: `KillMe` replaces them.
- Remove without a corpse: `Combatant_Base.RemoveFromGame()` (Clear All uses it). Killing wave units advances arena waves.
- Playable roster: no flag; combine `amPlayer`, `Char_Data.ReturnAmPlayerCharacter()`, `Game_Manager.currentCareer.chars_Core`, and the card's `DefaultFaction == Factions.Player`.
- Teams: change through `Squad_Main.AddMember` or `ChangeFaction`, never by writing `myTeam`. Playground spawns are `Factions.None` squads, which attack everyone outside their squad.

## AI state

- `Controller_Base.SetAlertStatus(C, AI_Wakeup.AlertStatus.Unaware | Hunting | Combat)` adds that state's modules. Attack a chosen target with `AI_Manager.SetKillTarget(C, target)` (claims through `AI_TargetClaimer`; no faction check). Back to unaware: release claims first (L-002).
- Stand still but fight: `wanderingDisabled = true` and `SetFrozen(true)`, which is the spawner's FixedInPlace option.
- Idle poses: `Combatant_Base.PlayAsEmote(AnimationClip, true, false)`, `AttemptEndEmote()`. Clips: `myAnimSet.IdleEmotes.myClips`, `LibraryStorageList.Library.AnimClips.Storage`. Walking, a new path or a wakeup ends the pose, so stop wandering while posing.
- Walk to a point: `AI_Manager.AssignWalkToPosition(C, point)`; arrival by distance with `myWaypoints` empty. To another room it only reaches the door: re-issue. `Orders_Manager.IssueOrder` handles routes across rooms.

## Mouse and UI

- Pick a character on screen: `Camera.WorldToScreenPoint` against `Input.mousePosition` (room-scoped game queries miss other rooms).
- Floor point under the mouse: the player body's `aim_Intended` (not updated while the controller is locked).
- Left click always attacks; there is no "mouse over UI" check. A prefix on `Controller_Player.RunControls_Attacks()` (protected: name it by string) that returns false while the mod owns the mouse, plus `LockAttackKey(0)` on the click (held until release).
- IMGUI (`OnGUI`, needs `UnityEngine.IMGUIModule`) is enough for lists and a name box. While a window takes typing, `LockController` the player so letters do not move or act; set `Cursor.visible = true` in `OnGUI` (the game hides it while aiming).

## Saving a scene

- No Playground save exists. Spawn with `Char_Data.CreateCharacter(StatCard_Base.ReturnCardByName<StatCard_Character>(card))`, `Combatant_Base.CreateCombatant(data, Cam_Main.currentRoom, ControllerType.NPC, pos, rot, false)`, `Squad_Main.CreateSquad(faction)` and `AddMember`. Weapons: `Catalog_Weapons.GenerateWeapon` then `Char_DataAssigner.GiveItemToCharacter` (grenades: `GiveThrownToCharacter`). Damage is `myTrauma`. Clear first with `InteractiveMode.ClearAll_DoIt()`.
- Write mod data outside `Mods/`: the loader reads every `.json` and `.txt` in a mod folder as a MadCard. `Application.persistentDataPath/<ModName>/` works; `UnityEngine.JsonUtility` (needs `UnityEngine.JSONSerializeModule`) handles `[Serializable]` classes with `List<T>` and `Vector3`.
- Mode check: `Game_Manager.currentGameMode == GameModes.Playground`.
