# Game recipes for Harmony and behaviour mods

Members of MADNESS: Project Nexus that mods change characters with, found in a decompile of beta v2.20.a (build 25615196) on 2026-10-21 for the Playground Director mod ([examples/SwapUnaware](../../../examples/SwapUnaware/README.md)). Names only. Re-check each with `decomp.py find <decompile> <Name>` after a game update. Status: playtested on the owner's PC on 2026-10-21 (no exception in Player.log) for the key check, freezing, removing, AI states, idle poses, patrol walking and the attack block; guard (FixedInPlace) and saving a scene are built but not yet exercised in play. Scenes in story levels and arenas (Playground Director 0.5.0, same build) are deployed, playtest pending.

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
- Clear corpses: `HaveRunKillMe` true and not `AmDespawning`, then `Decay_Manager.DestroyMe_Character(c)` (the dev console and arenas do this; it calls `DeSpawn`). `Decay_Manager.ClearAll_Character()` only `Object.Destroy`s the decay queue. Keep `amEssential`, serial-numbered, boss and player-squad bodies. Blood and debris: `ClearAll_Splats`, `_Holes`, `_Casing`, `_Debris`.
- Item cards by name: `StatCard_Base.ReturnCardByName<StatCard_Weapon>` finds only cards registered from `Stats/Weapon/<WeaponCategories>/` (plus mod cards). Level props such as the laborer's `N_Crate` live elsewhere; while the level is loaded, `Resources.FindObjectsOfTypeAll<StatCard_Item>()` finds them by name. `Catalog_Weapons.GenerateHeld(card)` takes either.
- Orders: `TeleportTo` completes one combat order only. `Orders_Manager.ClearMoveOrders(c, OrderUrgency.Idle or .Combat, ignorePermaOrders: true)` drops the other move orders and keeps persistent or repeating ones (a squad's follow order).
- Playable roster: no flag; combine `amPlayer`, `Char_Data.ReturnAmPlayerCharacter()`, `Game_Manager.currentCareer.chars_Core`, and the card's `DefaultFaction == Factions.Player`.
- Teams: change through `Squad_Main.AddMember` or `ChangeFaction`, never by writing `myTeam`. Playground spawns are `Factions.None` squads, which attack everyone outside their squad. Hostility is `Combat_Manager.FactionIsAttackable`: factions differ (or the attacker is None) and the squads differ, so a respawned squadmate must rejoin the live squad, not a new `CreateSquad` (Director 0.7.5). `AddMember` also takes a wave unit out of its wave unless the new squad has one.
- Emotes: `PlayAsEmote(clip, fade, false)` starts one; `InLoopingEmote()` says if a loop plays. The clip is not stored: read it from `myAnimator_Character.myOverride[Catalog_Animations.DefaultAnimatorClips[slot]]`, slot `Emote_0` when the private `emoteState` is 1, else `Emote_1` (they alternate).

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
- Write mod data outside `Mods/`: the loader reads every `.json` and `.txt` in a mod folder as a MadCard. `Application.persistentDataPath/<ModName>/` works; `UnityEngine.JsonUtility` (needs `UnityEngine.JSONSerializeModule`) writes strings, numbers, `Vector3` and lists of those, but silently drops every field whose type is a class from the mod's own assembly (loaded at runtime, unknown to Unity's serializer): nested records vanish with no error. Serialize each flat record on its own and join them yourself, as `SceneData.ToJson` in the SwapUnaware example does, or write the JSON by hand.
- Mode check: `Game_Manager.currentGameMode == GameModes.Playground`.
- Weapons and items (for a scene that resets them): loose ones are `Item_Held` (melee, ranged and thrown; `Item_Ranged` derives from it) with `myOwner` null, or a dead owner when still holstered on a body; each room also lists them in `Room_Main.listHeld`. Skip `myData.amRegistered` (a career's own weapon), `AmQuest()`, `AmMenuObject` and `AmSecondaryObject`. Lay one down with `Item_Held.CreateHeld(Catalog_Weapons.GenerateHeld(card), pos, rot, room)`; remove with `DeSpawn()`, which lets go of a held or thrown owner itself (a holstered one needs `owner.ClearItemFromSlot(item)` or `RemoveItemFromSlot(1, hand, false)` first). A loadout is `myItemsHeld` (slot 0), `Combatant_Character.myItemsStowed` (slot 1) and `myItemThrown`; leave items whose `CanBeDropped()` is false (they come with the card). Ammo: `Item_Ranged.myAmmo + myAmmoLimbo` and `myExtraMags`, set with `SetAmmo`/`SetMags`; melee durability `myMeleeAmmo`, `SetMeleeAmmo`. The Playground's Clear All despawns the current room's loose items but never puts any back.

## Levels, rooms and the career save

- Which level: `Game_Manager.currentGameMode`, `Stage_Manager.currentStage` (a card; null in some hubs; `FullName` to show) and `Game_Manager.GameTransitions.NameOrPathOfCurrentScene()` (path for .sobj scenes). There is no level index.
- Rooms have no id: match `name` plus `SerialNumber`, then nearest position, over `Room_Main.allRooms`. Save each character's `myRoom`; no lookup from a position exists.
- Story, arena and custom stages lock a room you leave (`amLockedOut`, GameObject off, temporary squads cleared). A room's own characters spawn when the player first enters it (`RoomBecomesFocus`). `TeleportTo(pos, forward, room)` on the player's body enters the room the same way.
- Never call `InteractiveMode.ClearAll_DoIt()` outside the Playground: it removes squadmates, bosses and scripted characters in the room.
- `RemoveFromGame()` fires every death callback: kill counters, `CharacterDies` events, wave kills, and `FactionDead` when the last of a faction goes (it latches). Spawn replacements before removing.
- Scripts find characters by serial number; a spawned character has none until `SetSerialNumber`. Leave characters with a serial, `amEssential`, the player squad (`Squad_Main.AmPlayerSquad`), bosses (`AmType_Boss()`) and cutscene actors (`myCutscene`) alone.
- Arena waves count squad rosters; a spawned unit joins the running wave with `WaveContainer.AddUnit(c)`. Pause is blocked between waves.
- `Game_Manager.SaveCareer()` runs on every stage start (`Stage_Manager.StageSelectedFromMenu`) and wave start and end, not on death or restart. Starting a story stage by code writes the career; switching Playground stages with `GameTransitions.LoadScene` does not. Keep spawned characters out of the player's squad outside the Playground (their ids would go into `IDs_Squad`).
- Every scene load clears all pause sources (`ClearPauseGame`) and every spawned character.
