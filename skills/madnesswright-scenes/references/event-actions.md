# Event Conditions and Actions (full list)

From the official Event System "Conditions and Actions" spreadsheet (2026-11-12f harvest, items 72-77; `https://docs.google.com/spreadsheets/d/1RYN4OIM1cvu2E422TRfWfcJJNmy1YE1pJWEYw8dGEs4/export?format=csv`, fetched 2026-11-12). Official text, but **unverified in game**: none of these has been tried in a scene by this skill. Card shape and Booleans/WorldChanges are in [stages-and-campaigns.md](stages-and-campaigns.md).

Shared field groups, written once here:
- **Who** = `Location` (SerialNumber of a Room or EventZone to search; no match means anyone anywhere) + `CharParam1.Faction` + `CharParam1.Target` (SerialNumber, Player or Card) + `CharParam1.SerialNumber` or `CharParam1.MadCard`.
- **Op** = `BoolOp` True, False or Toggle. **Cmp** = `Amount` (or `Int1`) + `Operator` Equals, NotEqual, Greater, Lesser, GEqual, LEqual.
- **SUBZERO=ALL**: an Action whose `Amount` is 0 or less applies to every qualified object found. Marked (ALL) below.
- **NO-RECHECK**: the Condition is true only the moment it fires; if other Conditions block the Event it is false until it fires again, and two NO-RECHECK Conditions cannot be combined. Marked (NR).
- Seat (a usually pre-filled spawn point: asylum tables, cloning vats) and NPC Zone (a zone that follows a character so the player can talk to them) are marked "Not yet implemented" in the sheet's notes.

## Conditions

| Condition | Fields |
|---|---|
| CharacterDies | Cmp; CharParam1 Faction/Target/SerialNumber/MadCard |
| FactionDead (NR) | Enum faction; Bool1 current Room only |
| MapStart | passes about 3 s after the scene loads |
| EnterRoom (NR) | Location = Room SerialNumber |
| BooleanIs / CheckWorldChange | String1 name; Bool1 true or false |
| IntegerIs | String1 integer; String2 second integer, or empty to use Int1; Operator |
| StageStatusIs | Enum Victory, Failure, Draw, None (None until SetStageStatus) |
| WaveCount / WavesRemaining | Int1 + Operator (Mission Waves) |
| WaveComplete (NR) | Bool1 any wave counts, not only Mission Waves |
| TimedEvent (NR) | String1 Timer or Announcement name |
| TrackedEvent | String1 name; Cmp |
| TasksRemaining | SerialNumber of the Tasks; Int1 + Operator |
| CheckStageWasCompleted | CardRef stage MadCard; Bool1 TRUE = completed, FALSE = never completed or played |

## Actions: units

| Action | Fields |
|---|---|
| SpawnUnit | Bool1 join an existing squad in the room; Bool2 ignore the card's default weapons; Amount; Location Door or Spawnpoint; CardRef Animation card for startup animations; CharParam1.MadCard, .Faction, .SerialNumber (given to spawns), .Awake (start alerted) |
| KillUnit | Bool1 remove instead of kill; Bool2 perma-death for those who would be Incapacitated; Who |
| HealSquad | Bool1 Medical Cabinet rules; Bool2 ignore dead squadmates |
| SwapFactions | Who; CharParam2.Faction the faction to swap to (the sheet's Bool1 row reads "Set the Faction", unclear) |
| WakeupUnits | Op (alert state); Who |
| RegenerateSquad | resurrects and heals the whole player squad |
| SendToDoor | Bool1 remove on arrival; Bool2 sprint; SerialNumber Door or Seat; Who |
| UnitSays | Enum Angry, Attack, BigAttack, Cheer, Laugh, Grunt, Death, Pain, Affirmative, Dialogue, Gasp, Buy, Sell, Interact; Bool1 narrate at the top like a cutscene; Bool2 interrupt narration; String1 line; String2 speaker name and CardRef Chatter card if no character is found; Who |
| SetAIModule | Op; String1 comma-separated module list; Who |
| MoveUnits (ALL) | Amount; String1 destination Spawnpoint or Door SerialNumber; Who (instant) |
| MakeInvincible (ALL) | Bool1 resurrect if dead; Bool2 Unbreakable (takes damage, cannot die); Amount seconds; Op; Who |
| EnableNPCZone | Op; SerialNumber NPC Zone; Who |
| FollowUnit (ALL) | Bool1 clear other orders; Amount; SerialNumber character to follow; Who |
| PickUpOrUse (ALL) | Bool1 clear other orders; Bool2 walk; Amount; SerialNumber Interactive or Weapon; Who |

## Actions: items and popups

| Action | Fields |
|---|---|
| SpawnItem | Bool1 apply ItemParam.StartVelocity; Bool2 sparkle on creation; Amount; Location Spawnpoint or Locker; ItemParam.MadCard, .SerialNumber, .RandomSpawn, .Registered (not junk), .AmQuest (respawns), .StartVelocity |
| KillItem | Bool1 despawn instead of break; Bool2 ignore Quest items; SerialNumber; Location (blank = everywhere) |
| GiveGear | Int1 slot 0 held, 1 stowed; Int2 hand 0 main, 1 off (off-hand moves to an empty main hand); Amount (max 1 each); Who; ItemParam.MadCard, .SerialNumber |
| RemoveGear | Bool1 ignore character qualifiers; Bool2 break armor or despawn weapon; SerialNumber and ItemParam.MadCard stack; Who |
| SetFlashlight | Amount; Op; Who. Scenes not lit for it look wrong |
| OpenLocker | SerialNumber |
| ClearAwayWeapons | Bool1 despawn now instead of decay; Location Room |
| ResetPlayerLoadout | reapplies the squad's loadouts |
| Notification | String1 text |

## Actions: world

| Action | Fields |
|---|---|
| EnableWorldObject | Bool1 also the GameObject; SerialNumber; Op |
| SetDoorLock | Bool1 open if possible; Bool2 force open; SerialNumber; Op |
| SetDoorConnection | Bool1 mutual; Bool2 lock sound; SerialNumber origin; String1 destination door; String2 scene (String1 is then a door in that scene) |
| GiveInteractiveTask (ALL) / GiveDestroyTask (ALL) | Bool1 one random PopupComplete each; Bool2 no repeats; Amount; SerialNumber; TaskInteractive / TaskDestroy |
| GiveZoneTask (ALL) | Amount; SerialNumber zones; TaskZone |
| LockInteractiveTask | SerialNumber; Op |
| ClearTasks | SerialNumber of the Tasks |
| SetSerialNumber (ALL) | SerialNumber; String1 new; Amount |
| SetTogglable | Bool1 + Int1 set a State-based Togglable to state Int1; SerialNumber; Op |
| FlashObject | Int1 frames; Int2 flashes; String1 hex colour; SerialNumber |
| SetHazardEnable / SetSpawnerActive / CameraAreaOn | SerialNumber; Op |
| PurgeSeat | SerialNumber of Seats |
| RunDynamicMethod | see below |
| SetRoomLockout | SerialNumber (blank = all Rooms); Op |

## Actions: variables, events, stages, missions

| Action | Fields |
|---|---|
| BooleanSet / SetWorldChange | String1 name; Op |
| IntegerSet | String1 integer; String2 second integer; MathOp; Bool1 randomise Int1..Int2; Int1 value if String2 empty; Int2 top of the range (scene-only) |
| SetStartScene | String1 scene; String2 door (where the career loads next time) |
| EnableCutscene | Bool1 also the GameObject; SerialNumber; BoolOp Enable, Disable, Toggle |
| PlayMusic / ResetMusic | String1 track (empty = stage default; ResetMusic has none); Bool1 match the playing position |
| StopMusic | Int1 fade seconds |
| SetStageMusic | String1 track, this scene only |
| SlowMo | Int1 0 slowest to 100 none (Int2 unused) |
| SetStageStatus, then EndStage | Enum Victory, Failure, None |
| Announcement | Enum Mission, Objective, ObjectiveComplete, Wave, Overworld; String1 name (fires TimedEvent); String2 title; String3 body |
| ClearAnnouncements / CancelAllTimers | none |
| TimerStart | Bool1 visible; Bool2 Enter skips; Int1 frames; String1 name |
| LoadStageWaves | Bool1 stop squads heat-seeking the player (arena) |
| SpawnNextWave / ClearAllWaves | none (ClearAllWaves cancels looping waves too) |
| PauseSpawning / ToggleNeverendingWave | Op; SerialNumber Squad Spawners |
| ForceBeginMission | SerialNumber MissionCards |
| RestartScene | none |
| GoToScene | Enum WipeSide, FadeOut, DramaticFade, FastOutSlowIn; Bool1 apply it; Bool2 erase squad; String1 scene (blank = hub); String2 door |
| SetQuestStatus | Op; SerialNumber Door, Item or Character; Location |
| SetQuestMarker | Op; Enum Default, Kill, Help; Bool1 visible outside the room; SerialNumber |
| SetLoadout / SetWardrobe | Bool1 remove; CardRef card (career unlocks) |

Arena order (unverified in game): `LoadStageWaves`, then `SpawnNextWave` per round, gated by `WaveComplete` or `WavesRemaining`.

## RunDynamicMethod and RunLua (item 76)

`RunDynamicMethod` runs a method on code attached to the objects with `SerialNumber`, or a static method when `Location` names the class. `String1` is the method name; `String2` the comma-separated arguments, which also pick the overload: `true`/`false`, `'strings'` in apostrophes, `5`, `3.14`, floats with a decimal point (`5.0`, never `5` for a float), vectors as 2-4 floats in parentheses separated by spaces `(0.5 1.0 5.8)`, enums as `Faction.AAHW`. This is the bridge from Events to a Harmony or C# mod (madnesswright-modtools references/harmony.md). `RunLua` does nothing alone; Lua mods hook it, and Lua is marked obsolete in the Master Document. Unverified in game.
