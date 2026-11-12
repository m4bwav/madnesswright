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
- `Leave_Default_ToDoor`: the door SerialNumber to arrive at, so a repeat run can start somewhere else. A stage may name itself as its `Leave_Default_ToStage` to loop forever. A one-stage campaign with no target goes back to the main menu when the player wins.
- `Hubs` on the Campaign does two things: the game autosaves the player there, and shows the hub's name across the top of the screen on entry. A stage that only plays the credits or a flashback is `AmHub` true but left out of Hubs, so the save never reopens it.
- A Stage marker (Markers) in a hub launches a stage card by Filename. `Prerequisites` and `Forbidden` are WorldChanges that show or hide it; a stage whose prerequisite stages are not beaten shows red and locked; `QuestIfUnbeaten` puts the go-here arrow on it.
- Missions live on the stage card (below).

Existing stages can also be edited with MadCards alone (re-ordering Arena waves, swapping the Story squad) without SceneTools.

## Cutscenes

A Cutscene marker in the room, linked to a CutsceneCard through `AttachCutsceneCard`. Actors are found by SerialNumber (player squad: `PSQUAD0`, `PSQUAD1`, ...). Marks are waypoints by index; ViewCams are cameras by index, driven through a `ToggleMe` trigger in their Animator. Each Step is one beat: a line, moves to Marks, actions, sounds; `ActorSerial` takes a comma list (dialog takes one).

From the official video tutorial (https://youtu.be/u7VV0N33Sx0):
- Marks and the ViewCam are children of the Cutscene, so moving the marker moves them too. A new Mark must also be added to the Marks list; `Mark` in a Step is its list index from 0.
- A WorldChange set on the Cutscene makes it play once per career, until an event sets it false; meant for hubs, not replayable stages. A higher `CutscenePriority` interrupts a lower one (the author was not certain of the direction).
- Start by zone (an Event zone in the same room, linked), room or existence. `PlayerOnlyActivation` with `AllActorsRequired` is the usual default; steps of a dead actor are skipped. Turn off controls for actors and player when the camera takes over. `Unskippable` covers the whole cutscene and can be overridden per Step; `DialogueAsNarration` shows lines at the top instead of over heads; `AllowOutsideActors` lets unlisted serials speak; `ForceNewPlayerSquad` replaces the squad with the listed character cards (existing ones are kept, new ones must be placed).

## EventCards

Same file formats as MadCards (`.json` standard or `.txt` shorthand, can share a file). `Type` `Event`, `Filename`, `Conditions` [], `Actions` [], `RepeatEvent`. No Root; no override: the first card with a Filename wins, so make Filenames unique (suffix them).

```
{ "Type": "Event", "Filename": "MyLevel_HealOnFinal_01",
  "Conditions": [ { "Type": "EnterRoom", "Location": "FinalRoom_4", "AmCheckpoint": true, "TrueForever": true } ],
  "Actions": [ { "Type": "HealSquad", "Bool1": true, "Bool2": false } ] }
```

- Events live for one scene. Booleans (`BooleanSet`, `BooleanIs`) reset on leaving; WorldChanges (`SetWorldChange`, `CheckWorldChange`) persist in the career across stages; TimedEvents from `TimerStart` or Announcements; TrackedEvents count Task completions, compared with an Operator (Equals, NotEqual, Greater, Lesser, GEqual, LEqual).
- Every Condition and Action with its fields, SUBZERO=ALL and NO-RECHECK, wave actions, `GoToScene` and `RunDynamicMethod`: [event-actions.md](event-actions.md) (official sheet, unverified in game).
- Briefings, Announcements and `UnitSays` lines: write in the game's voice from the official STORY MODE transcripts ("All Mission Briefings" doc and the "(FORMAT)" doc: BRIEFING / OBJECTIVES / MISSION ENGAGED / MISSION COMPLETE) and ARENA MODE transcripts; links in madnesswright references/sources.md (2026-11-12f harvest, item 80; community-kept copies of official text).
- `AmCheckpoint` stops later Conditions being checked until this one completes; `TrueForever` keeps a met Condition met (needed for NO-RECHECK ones like EnterRoom or a finished wave).
- Registering: name the EventCard in the scene's SceneRules, or put its Filename in a Task's `TaskEvent` with `RunTaskEvent` on (Interactives, Event Zones, Destructibles; Conditions are skipped then). `Time_Goal` 0 means instant use; `AutoEvent` `Locker` makes a Locker or Vendor addon work.

## Missions

On the stage card. `Conditions_Start` (at least one) starts it; `Actions_Exists`, `Actions_Start`, `Actions_Success`, `Actions_Failure` fire automatically. At least one Act; each Act has Objectives (holders of Conditions) with `ShowProgress`, `Hidden`, `CanFail`. Missions last one scene and reset on death: keep each one finishable in a single scene.

## Arena (SceneRules on the scene, 2026-08 tools, testing)

Start interactive (the game adds the start-waves action), Siege Targets (Destructibles or Interactives), victory cutscenes, Staging Grounds zones, PX_SiegeRepair, and on each room's RoomObj: `ArenaSafeRoom`, `PatrolPaths` and `GuardPoints` (Infiltration), `ToysToResetOnReEnter`. A custom arena stage replaces an existing one; there is no free slot yet.

Custom stage music (2026-11-12b harvest, item 28; community, unverified in game): a 2D FMOD event with a loop region shows in the Playground Radio and can be set as a Stage_Arena's music. Steps in madnesswright-modtools references/sounds-and-fonts.md.

Beta-only (2026-11-12e harvest, items 63, 69; SceneTools_Testing commit messages, unverified in game, names need confirming): Progress and Backtrack Blockers stop and later allow progress across stages (like the Outskirts junkyard crawl; fields unknown). `Tool_MoviePlayer` plays a movie in a cutscene, with a cutscene camera AnimController. `Effect_ExplosionGenerator` (scenetools-rules.md) suits event-fired set pieces.
