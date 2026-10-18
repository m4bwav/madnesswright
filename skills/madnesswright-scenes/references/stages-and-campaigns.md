# Stages, campaigns, cutscenes, events and missions

Source: SceneTools Overview Parts 5 to 7 and the Event System Overview (official Google Docs), read 2026-10-17. Exact Type names and the full field lists are in MadCard Maker (MCM) and the Event System's Conditions and Actions spreadsheet (linked from the Event doc); mark any name not shown here as "confirm in MCM".

## Campaign card

Describes the new career (save) a player starts: name, description, icon, `StartStage` (Filename of the first Custom Stage card) and `Hubs` (Filenames of overworld stages to fall back to). It does not control what happens after the start. Campaigns use Story rules: no Siege or Infiltration.

## Custom Stage card

The game checks stages and may drop invalid ones.

- `SceneName`: the Unity scene to load. Invalid: the stage is not recognised.
- `RequiredCharacters`: at least one Character Set (character plus weapon and wardrobe loadouts) or the stage cannot be played. Never the same Character card twice in a squad: make a copy card with the same Root instead.
- `BonusCharacters`: extra players (same rule).
- `AmHub`: overworld stage with no start menu, no win or loss. List it in the Campaign's Hubs.
- `Leave_Default_ToStage`: where to go after the stage; `Leave_Victory_ToStage`, `Leave_NeverWon_ToStage` and the other `Leave_` values make branches. No valid target: the campaign ends (back to the last Hub or the menu).
- Missions live on the stage card (below).

Existing stages can also be edited with MadCards alone (re-ordering Arena waves, swapping the Story squad) without SceneTools.

## Cutscenes

A Cutscene marker in the room, linked to a CutsceneCard through `AttachCutsceneCard`. Actors are found by SerialNumber (player squad: `PSQUAD0`, `PSQUAD1`, ...). Marks are waypoints by index; ViewCams are cameras by index, driven through a `ToggleMe` trigger in their Animator. Each Step is one beat: a line, moves to Marks, actions, sounds; `ActorSerial` takes a comma list (dialog takes one).

## EventCards

Same file formats as MadCards (`.json` standard or `.txt` shorthand, can share a file). `Type` `Event`, `Filename`, `Conditions` [], `Actions` [], `RepeatEvent`. No Root; no override: the first card with a Filename wins, so make Filenames unique (suffix them).

```
{ "Type": "Event", "Filename": "MyLevel_HealOnFinal_01",
  "Conditions": [ { "Type": "EnterRoom", "Location": "FinalRoom_4", "AmCheckpoint": true, "TrueForever": true } ],
  "Actions": [ { "Type": "HealSquad", "Bool1": true, "Bool2": false } ] }
```

- Events live for one scene. Booleans (`SetBoolean`, `BooleanIs`) reset on leaving; WorldChanges (`SetWorldChange`, `CheckWorldChange`) persist in the career across stages; TimedEvents from `TimerStart` or Announcements; TrackedEvents count Task completions, compared with an Operator (Equals, NotEqual, Greater, Lesser, GEqual, LEqual).
- `AmCheckpoint` stops later Conditions being checked until this one completes; `TrueForever` keeps a met Condition met (needed for NO-RECHECK ones like EnterRoom or a finished wave).
- Registering: name the EventCard in the scene's SceneRules, or put its Filename in a Task's `TaskEvent` with `RunTaskEvent` on (Interactives, Event Zones, Destructibles; Conditions are skipped then). `Time_Goal` 0 means instant use; `AutoEvent` `Locker` makes a Locker or Vendor addon work.

## Missions

On the stage card. `Conditions_Start` (at least one) starts it; `Actions_Exists`, `Actions_Start`, `Actions_Success`, `Actions_Failure` fire automatically. At least one Act; each Act has Objectives (holders of Conditions) with `ShowProgress`, `Hidden`, `CanFail`. Missions last one scene and reset on death: keep each one finishable in a single scene.

## Arena (SceneRules on the scene, 2026-08 tools, testing)

Start interactive (the game adds the start-waves action), Siege Targets (Destructibles or Interactives), victory cutscenes, Staging Grounds zones, PX_SiegeRepair, and on each room's RoomObj: `ArenaSafeRoom`, `PatrolPaths` and `GuardPoints` (Infiltration), `ToysToResetOnReEnter`. A custom arena stage replaces an existing one; there is no free slot yet.
