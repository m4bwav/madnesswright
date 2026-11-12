# Madness technique harvest, 2026-11-12 (round f)

Goal: the same as [round e](2026-11-12e-madness-technique-harvest.md). I worked round e's leads and grepped skills/ for every name below before counting it; none of the names in items 72-80 appear anywhere under skills/ (stages-and-campaigns.md already has CharacterDies, FactionDead, EnterRoom, BooleanIs, CheckWorldChange, TimedEvent, TrackedEvent, HealSquad, SetWorldChange, TimerStart, Announcement, SlowMo, SetSerialNumber and the NO-RECHECK note). Item numbers continue from 71. Nothing here has been built or tested in game.

Fetches were one try each: curl with a browser user agent (`export?format=csv`, `export?format=html`, Drive folder pages) and `gh api` for the commit log.

Sources:
- F1: Event System "Conditions and Actions" spreadsheet (official, linked from E1): https://docs.google.com/spreadsheets/d/1RYN4OIM1cvu2E422TRfWfcJJNmy1YE1pJWEYw8dGEs4 (`export?format=csv` works, 27 KB)
- F2: MPN_SceneTools_Testing commits 2025-01-04 to 2025-04-21 (22 commits): https://github.com/MPN-Modders/MPN_SceneTools_Testing/commits
- F3: D1's Drive folders, full ids recovered from D1's HTML export, all at `https://drive.google.com/drive/folders/<id>` and publicly listable: 13N2tXfez6DyXuYBXwv5ckuGkcaIaoq1Y (FMOD tutorial screenshots), 1FztFgPoyyrXtZFt7VIo1WtptykoKKjQp (MPN Official Misc assets), 1ln1t2WmTIT8BhQihTdDHPE5vvnlMbV1W (ARENA MODE), 1zEPjlzi-pgBnmCeLdrxMb1CeY0PR8j2X (STORY MODE).

Confidence labels as in round b.

## Techniques (ranked by usefulness)

72. **Full Event action list (Units, Items, World).** Units: `SpawnUnit`, `KillUnit`, `SwapFactions`, `WakeupUnits`, `RegenerateSquad`, `SendToDoor` (Bool1 remove on arrival, Bool2 sprint, Location, SerialNumber of Door or Seat, CharParam1.Faction/.Target), `UnitSays` (Enum chatter type, Bool1 narrate at the top like a cutscene, Bool2 interrupt, String1 line, String2 speaker name if no character, CardRef Chatter card), `SetAIModule`, `MoveUnits`, `MakeInvincible`, `EnableNPCZone`, `FollowUnit`, `PickUpOrUse`. Items: `SpawnItem`, `KillItem`, `GiveGear`, `RemoveGear`, `SetFlashlight` (warns about dark scenes), `OpenLocker`, `ClearAwayWeapons`, `ResetPlayerLoadout`. Popups: `Notification` (String1). World: `EnableWorldObject` (Bool1 also the GameObject, BoolOp True/False/Toggle), `SetDoorLock`, `SetDoorConnection` (Bool1 mutual, Bool2 lock SFX, SerialNumber origin, String1 destination door, String2 scene), `GiveInteractiveTask`, `LockInteractiveTask`, `GiveZoneTask`, `GiveDestroyTask`, `ClearTasks`, `SetTogglable`, `FlashObject` (Int1 frames, Int2 flashes, String1 hex colour), `SetHazardEnable`, `SetSpawnerActive`, `CameraAreaOn`, `PurgeSeat`, `SetRoomLockout`. Skill: madnesswright-scenes (stages-and-campaigns.md, new "Actions by group" table). Source: F1. Confidence: official spreadsheet. In-game test: none.

73. **Variables, music, stage flow and mission actions.** `BooleanSet` (the skill writes `SetBoolean`; F1 says BooleanSet, so fix it), `IntegerSet` (String1, String2 second integer, MathOp, Bool1 randomise with Int1..Int2), `SetStartScene` (String1 scene, String2 door), `EnableCutscene`, `PlayMusic` (empty = stage default), `StopMusic`, `ResetMusic`, `SetStageMusic`, `SetStageStatus` then `EndStage`, `ClearAnnouncements`, `CancelAllTimers`, `ForceBeginMission`, `RestartScene`, `GoToScene` (Enum WipeSide/FadeOut/DramaticFade/FastOutSlowIn, Bool1 apply transition, Bool2 erase squad, String1 scene or blank for hub, String2 door), `SetQuestStatus`, `SetQuestMarker` (Enum Default/Kill/Help, Bool1 visible outside the room), `SetLoadout` and `SetWardrobe` (CardRef, Bool1 remove; career unlocks). Skill: madnesswright-scenes (stages-and-campaigns.md). Source: F1. Confidence: official. In-game test: none.

74. **Arena wave actions.** `LoadStageWaves` (Bool1 stops squads heat-seeking the player), `SpawnNextWave`, `PauseSpawning` and `ToggleNeverendingWave` on Squad Spawners (SerialNumber, BoolOp), `ClearAllWaves` (looping waves too). Skill: madnesswright-scenes (Arena section; pairs with item 71). Source: F1. Confidence: official. In-game test: none.

75. **Conditions the skill lacks.** `MapStart` (passes about 3 s after load), `IntegerIs`, `StageStatusIs` (None until set), `WaveCount`, `WaveComplete` (NO-RECHECK), `WavesRemaining`, `TasksRemaining`, `CheckStageWasCompleted`. Skill: madnesswright-scenes (stages-and-campaigns.md). Source: F1. Confidence: official. In-game test: none.

76. **RunDynamicMethod and RunLua.** `RunDynamicMethod` calls a method on code attached to objects (String1 method name; String2 parameters, which also pick the overload; SerialNumber targets; Location = class name for a static method). This is the bridge from Events to Harmony/C# mods. `RunLua` does nothing alone; Lua mods hook it (D1 marks Lua obsolete). Skill: madnesswright-modtools (Harmony code) and madnesswright-scenes. Source: F1. Confidence: official, untested. In-game test: none.

77. **SUBZERO=ALL and the Seat and NPC Zone terms.** An Action with Amount 0 or less applies to ALL qualified objects. A "Seat" in events is a usually pre-filled spawn point (asylum tables, cloning vats); an "NPC Zone" follows a character so the player can interact with them. Skill: madnesswright-scenes (stages-and-campaigns.md key). Source: F1 KEY and NOTES rows. Confidence: official. In-game test: none.

78. **Early SceneTools objects not in the skills.** `Hazard_Splosive`, `Marker_Boss` (2025-04-21); AutoShooter, Hazards, GetOut-Zones, Debris Generators (2025-04-18); `AnimEvents_Sound`, `CustomObj_Destructible` (sample `CO_Destructible`), `Obj_Debris`, `Obj_Disposable`, `Obj_Toy`, `Effect_MaterialScroller`, `Effect_RendererVanisher` (2025-04-04); `Marker_Dummy` (scenery characters driven by your own Mecanim controller, 2025-02-13); Glow Effect ("80s Lights") and scene Sounds (only loops play on the first frame); Platforms and Automators for Custom Togglables; PatrolParameters/Paths on character spawners; Locker and Vendor addons on Custom Interactives (they take a SerialNumber); Removers (author: untested); Cover Points; `AnimEvent_Interact` for custom interactive animators; Lockers get Tasks; an empty Door type. Skill: madnesswright-scenes (scenetools-rules.md). Source: F2. Confidence: single source (commit text). In-game test: none.

79. **AddonGroup TetherTo and new AnimEvents.** `MadCard_AddonGroup` takes `TetherTo`, a list of other AddonGroup names to merge its Addons into, so several mods extend one AddonGroup with no override. AnimEvents: `BuffEvent` takes any buff type, with `SetBuffLifespan(int)`; `DoTEvent` takes any DoT MadCard. Skill: madnesswright (madcards.md AddonGroup; pairs with item 55) and madnesswright-modtools (animation-events.md). Source: F2 2025-02-13. Confidence: single source from the developer; beta then, likely live now. In-game test: none.

80. **Official dialogue and briefing transcripts for writing in voice.** STORY MODE folder (per-area folders 0. FLASHBACKS to 7. ABANDONED, S-3LF EATER, EXTRAS; "All Mission Briefings" doc 1kA-pS-YbIJ3UrI5mlMPTSGJ2MNI7c2PrRr0CLx6qJoE; "(FORMAT)" doc 1xwLvuWlGnc0sCJ43KGrqRh9cAcK383SDjcFEshePCeE showing the BRIEFING / OBJECTIVES / MISSION ENGAGED / MISSION COMPLETE layout) and ARENA MODE folder (IMPRINT 1, IMPRINT 2, Contracts, Arenas, Misc; INDEX doc 12AYsoek_0l5ahKePKSfwxdzLKAbIPZ5zwFSZHzhGqzE). Use them as the style reference for `UnitSays` lines, Announcements and mission briefings. "MPN Official Misc assets" holds .blend, .unitypackage and Textures folders (Nov 2024). Skill: madnesswright (references/sources.md) and madnesswright-scenes (writing briefings). Source: F3. Confidence: community-kept transcripts of official text; the assets folder is labelled official. In-game test: none.

## Leads not yet read

- Prefab and tooltip text in MPN_SceneTools_Testing for exact field names of items 61-71 and 78 (`gh api repos/MPN-Modders/MPN_SceneTools_Testing/contents/...`); not done this round.
- F1 sub-field rows for actions not quoted above (SpawnUnit, GiveGear, SetAIModule and others): re-fetch the CSV and copy the full table when building item 72.
- Cl4ssic's armor tutorial is a YouTube video, https://youtu.be/GAeAC3uuCoc (not a Drive file); needs a transcript tool.
- MPN Official Misc assets subfolders (1gb7UmTs3swFuU1VYqTdEpzTr4TvQXbXA, 1-CkSfl9mQI52oASY-2sIHPQoC4EpNWmM, 17oMOEaXn5exvkdYImC5JejmkK-Pvs5Pr): file lists only.
- Steam discussions and top Workshop items for app 488860, GameBanana, ModDB, Reddit: not reached this round.

## Sources that failed or gave nothing

- F3 FMOD tutorial screenshots folder: seven PNGs, no text.
- ARENA MODE and STORY MODE folders: dialogue transcripts, no modding techniques beyond item 80.
- F2 2025-03-06 commits: repo cleanup only (deleted Library, Logs, .sln); the 2025-01-04 initial commits are empty.
