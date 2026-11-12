# Madness technique harvest, 2026-11-12 (round e)

Goal: the same as [round d](2026-11-12d-madness-technique-harvest.md). I worked round d's five leads and grepped the skills for every name below before counting it (Hazard_WaterZone, GlobalFog, NoIncap, NoDeadZoneDamage, BadBlock, Proxy_Seat, CO_Seat, CollisionDamageMult, ImpactThreshold, AdditiveForce, DriverActions, FaceTowards, Effect_ExplosionGenerator, Tool_MoviePlayer, StartPoints, Lockdown, Uniforms, Blockers: none in references/; NoIncap and BadBlock appear only in madnesswright-scenes/RESEARCH.md). Item numbers continue from 60. Nothing here has been built or tested in game.

Every fetch was one try: curl with a browser user agent for the Google Doc (`export?format=txt`) and the GitHub REST API (`gh api`) for commit logs.

Sources:
- E1: Event System Overview (official Google Doc): https://docs.google.com/document/d/1aLz0hngXuPbw2UbXwU6yontyCwL7mzKk789GHfMTzJM
- E2: MPN_SceneTools_Testing commit log (beta SceneTools, latest 2026-09-02): https://github.com/MPN-Modders/MPN_SceneTools_Testing/commits
- E3: MPN_ModTools_Testing commit log: https://github.com/MPN-Modders/MPN_ModTools_Testing/commits

Confidence labels as in round b. All E2 items are from commit messages (one line to a few bullets each) on the *testing* repo, so they are beta-branch features; confirm exact names in the prefabs and tooltips before writing them into a card.

## Techniques (ranked by usefulness)

61. **Water zones and per-room fog.** `Hazard_WaterZone` adds water to a stage; `GlobalFog` on a room's `Camera_Point` sets that room's fog, and the [Sync Visuals] button copies it to the Dev_Cam when the room is synced. Distortion shaders and normal maps ship with it. Skill: madnesswright-scenes (scenetools-rules.md, hazards/visuals). Source: E2 "Water & Fog Update" (2025-12-04). Confidence: single source (official repo, commit text). In-game test: none.

62. **Lockers on Destructibles.** A Locker addon can sit on a Destructible and drops its items when destroyed. Skill: madnesswright-scenes (scenetools-rules.md, CO_ objects). Source: E2 2025-12-04. Confidence: single source. In-game test: none.

63. **Progress and Backtrack Blockers.** Blockers that stop and then allow progress across stages, like the Outskirts junkyard crawl. Skill: madnesswright-scenes (stages-and-campaigns.md). Source: E2 2026-07-29. Confidence: single source; field names not in the commit. In-game test: none.

64. **Vessel tuning: CollisionDamageMult, ImpactThreshold, collision exemption, AdditiveForce.** Vessels take `CollisionDamageMult` and `ImpactThreshold`; a Vessel can ignore collision with its linked seat user; a DriverAction's `AdditiveForce` adds force to the velocity instead of setting it, so several DriverActions can drive one Rigidbody. VesselAnimationEvents exist alongside. Skill: madnesswright-scenes (scenetools-rules.md, new Vessels section; RESEARCH.md only dates the feature). Source: E2 2026-01-02 to 2026-01-09. Confidence: single source. In-game test: none.

65. **Seats: Proxy_Seat and CO_Seat samples, reticle in seat, FaceTowards limit.** Samples for seats ship as `Proxy_Seat` and `CO_Seat`; the reticle can be turned on while seated (turret seats); `FaceTowards` takes a limit. Skill: madnesswright-scenes (scenetools-rules.md; PX_Seat is listed, these are not). Source: E2 2025-11-25, 2026-01-12. Confidence: single source. In-game test: none.

66. **Character traits NoIncap, NoDeadZoneDamage, BadBlock, NoRanged.** NoIncap lets player squadmates truly die; NoDeadZoneDamage ignores dead zones (the S-3LF Eater has it); BadBlock blocks without the skill; NoRanged (moved to the end of the trait list, so index-based data may shift). Skill: madnesswright (madcards.md character traits) and madnesswright-scenes encounter-design.md. Source: E2 2025-07-29, 2025-09-17, 2025-11-06. Confidence: single source. In-game test: none.

67. **Uniforms on Marker_Character.** Character markers can carry Uniforms (outfits per placed character). Skill: madnesswright-scenes (scenetools-rules.md markers). Source: E2 2026-09-02. Confidence: single source; fields unknown. In-game test: none.

68. **Effect_ExplosionGenerator.** A scene effect that "creates the booms": scripted explosions without a weapon. Skill: madnesswright-scenes (scenetools-rules.md effects; stages-and-campaigns.md for event-fired set pieces). Source: E2 2025-09-25. Confidence: single source. In-game test: none.

69. **Tool_MoviePlayer and the cutscene camera AnimController.** A movie-player tool plus a camera animator controller for cutscenes. Skill: madnesswright-scenes (stages-and-campaigns.md cutscenes). Source: E2 2025-06-19. Confidence: single source. In-game test: none.

70. **Custom doors: StartPoints (renamed from SpawnPoints), door addon effects, access lights, lockdown.** The custom-door variable `SpawnPoints` became `StartPoints` (old guides use the old name); doors take addon effects and access lights; a 2026-07-06 "Door Lockdown Update" adds lockdowns (details not in the message). Stage Start Markers came 2025-06-06. Skill: madnesswright-scenes (scenetools-rules.md doors). Source: E2 2025-06-06, 2025-06-09, 2025-06-23, 2026-07-06. Confidence: single source. In-game test: none.

71. **Camera Zones have tooltips; Arena setup is unfinished.** Camera Zones exist with tooltips (2026-01-29); "ARENA TEST SETUP" (2026-07-11) is marked by the author as unfinished, so custom Arena work on beta may change. Skill: madnesswright-scenes (RESEARCH.md / scenetools-rules.md caveat). Source: E2. Confidence: single source. In-game test: none.

## Leads not yet read

- E2 commits older than 2025-04-22 (only the newest 30 were read) and the prefab/tooltip text in the repo for exact field names of items 61-71.
- Event System doc's Conditions and Actions spreadsheet (linked from E1) for any action names newer than 2026-10-17.

## Sources that failed or gave nothing

- E1 "Mission card functions": the current doc text (Part 5, Missions and MissionCards: Conditions_Start, Actions_Exists/Start/Success/Failure, Acts, Objectives with ShowProgress/Hidden/CanFail) matches stages-and-campaigns.md "Missions" already; no new functions.
- Zep's and Bagmetti's YouTube videos: yt-dlp is not installed and no video URLs for them are recorded in the notes; not tried.
- Cl4ssic's armor tutorial and the four Drive folders: only the id prefixes (13N2tXfe, 1FztFgPo, 1ln1t2Wm, 1zEPjlzi) are on disk, not the full ids; needs D1's HTML export re-read to recover them.
- Debug Mode flag: no decompile folder is recorded in ai-docs (decomp.py writes only to a chosen --out); owner-blocked until the owner runs `decomp.py decompile` and names the folder.
- E3 MPN_ModTools_Testing: three commits (2026-03-18 to 2026-04-07: welcome, README, cleanup); no feature notes.
