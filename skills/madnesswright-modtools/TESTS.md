# Tests: madnesswright-modtools

Test runs for [SKILL.md](SKILL.md). Cases live in `evals/evals.json`. A failure that taught something is a lesson in [LEARNINGS.md](LEARNINGS.md); a fix it caused is logged in [CHANGELOG.md](CHANGELOG.md) with `because: T-...`; research it triggered is in [RESEARCH.md](RESEARCH.md); counts and the failing list are in `evergreen.json` under `tests`. Rules: MAINTENANCE.md (testing section) and the plugin's `protocol/TESTING.md`.

A test passes on evidence (a tool call in the trace, a file, a marker, a log line), never on the transcript's claim that something was done.

Entry shape: `### T-YYYYMMDD-n · date · harness · env · passed/total`, then one line per failing case (`id · kind · class · what the evidence showed`), then `led to:` (L-, C-, R- ids or none). Newest first. Budget 150 lines; archive older runs to `TESTS-ARCHIVE.md`.

## Runs

### T-20261028-1 · 2026-10-28 · harness only, not played · dev-machine, Director 0.10.2 built against build 25615196 · 0/0 live
- live-freeze-takeover · not played · the requester's report on 0.10.1 (stuck in a body after F5 and F2) reproduced in `tests/DirectorFreezeChecks` before the fix, on both swap paths; 0.10.2 passes it. Play owed: in the Playground, F5 onto someone, F2, walk, F2; then F2, F5 onto a frozen body, walk, F8 home, F2. Back up Player.log first.
- led to: L-007, Director 0.10.2

### T-20261024-3 · 2026-10-24 · live play by the owner and the requester · owner's PC, Director 0.8.0 deployed, build 25615196 · 2/2 (two players)
- live-two-player-join · pass · log `player 2 joined on pad 0`, `player 2 now plays Deimos [2656]`; player one took a MERC and went back (`now plays` lines both ways).
- live-two-player-scene · pass · story scene "bob" on The Industrial Sector: file `"version":6`, Deimos `"player":2`; load logged `Deimos played by player 2: put back, moved 15.6 m; 3 of 3 weapons reset` and `1 played by other players put back, 10 moved back, 0 respawned, 0 removed`. No exception in the log. The owner reported the test passed. Evidence: `Player-20261024-two-player-080.log`, `scene-bob-20261024.json` in the owner's backups.
- led to: Workshop update to 0.8.0

### T-20261024-2 · 2026-10-24 · live play by the owner · owner's PC, Director 0.7.5 deployed, build 25615196 · 2/2 (scene load)
- live-f11-animations · pass · Playground scene "animation": file `"version":5`, `"emote":"anim_cookstation_stir"`; log `AAHW1_Agent plays anim_cookstation_stir again`, `1 of 1 saved animations playing again`. The slot choice in `CurrentEmote` names the right clip.
- live-f11-teams · pass · every load logged `0 pairs of saved squadmates are hostile to each other` (Playground, and story "what" on The Boom Factory: 4 MERCs respawned into one squad, Deimos put back in the player squad). No `rejoined` line was needed. The owner saw no fighting.
- Side notes: drift 0 of 5 on the first story load (was 9 to 14 of 15 on 0.7.4); 1 of 2 in the Playground (the posed agent 13.2 m off). A second story load was followed by a level exit inside 2 s, so its 2 s check reported 5 of 5 gone: the check runs after a scene change. One NullReferenceException is the game's own (`Target_Main.SweetSpot_Reconfigure` on player disable during unload). Evidence: `Player-20261024-075pass.log`, `scene-what-20261024.json` in the owner's backups.
- led to: none (passes; the after-scene-change report is owed)

### T-20261024-1 · 2026-10-24 · live use by the requester (report via the owner) · requester's play, Director 0.7.4 from the Workshop · 0/2 (scene load)
- live-f11-animations · fail · characters in the middle of an animation at save were no longer doing it after a load. No log or scene file from that play on the owner's PC (newest Player.log 2026-10-22). Code: version 4 records stored no emote.
- live-f11-teams · fail · teammates attacked each other after a load, not every time. Code: a respawned character got a new squad (`Squad_Main.CreateSquad`) even when saved squadmates survived; `Combat_Manager.FactionIsAttackable` makes different squads foes when the faction is None (Playground spawns).
- led to: Director 0.7.5 (built clean, deployed, not played)

### T-20261023-2 · 2026-10-23 · headless `claude -p` (Sonnet) per run, `--plugin-dir` on the branch worktree, `madnesswright@mark-local` disabled; each run in an empty folder · dev-machine · 3/3, baseline fail
- outcome-3 (new) · pass ×3 · each SlugGun/PLAN.md has the matrix with a niche column filled from the vanilla shotguns' cards (marked to confirm in MCM), the reskin-or-new-numbers decision argued, and a hit-feedback line in the playtest step.
- outcome-3 baseline (no plugin) · fail · wrote no file; asked for the Project Nexus project folder instead.
- led to: none

### T-20261022-10 · 2026-10-22 · evergreen-tester subagents (Sonnet), skill read from the branch · dev-machine · 3/3, baseline fail
- outcome-2 (new) · pass ×3 · each export_char.py passed a stdlib checker (the FBX call, `add_leaf_bones=False`, `bake_anim_use_all_actions=True`, scale applied alone or unit scale, the Rigify bone count handled): s1 built a DEF-only skeleton and baked onto it, s2 and s3 used deform-only export and warned. The checker first failed s2 for passing the setting through a variable (`use_armature_deform_only=DEFORM_ONLY`); a correct reply, so the regex was widened and re-tested on all four files and a wrong one.
- outcome-2 baseline (no skill) · fail · leaf bones off and one take per action, but `FBX_SCALE_ALL` with no scale step, and a comment saying deform-only drops Rigify's control bones (278 still export).
- led to: C-20261022-2

### T-20261022-9 · 2026-10-22 · live use by the owner · owner's PC, beta 2.20.a · 1/1 (blood on load)
- live-f11-blood · pass · Director 0.7.4 in The Industrial Sector, "Story_IndustrialSector_SOUTH": saved "outside" (15 characters), four loads: `56`, `66`, `89`, `275 newer blood splats, bullet holes, casings and debris cleared`, bodies 3/3/4/7 cleared, floor 8 of 8 back, 3 of 3 of your weapons reset, no exception. The owner: "Looks good".
- Drift check: 9 to 14 of 15 off their spot 2 s after each load (Combat ones up to 15 m); not seen as a problem by the owner.
- led to: Workshop update to 0.7.4

### T-20261022-8 · 2026-10-22 · live use by the owner · owner's PC, beta 2.20.a · 1/1 for 0.7.3's fixes, blood not reset
- live-f11-bodies-2 · pass · Director 0.7.3 in The Boom Factory, "Room1 - linear": saved "ambush" (four `Merc0_Grunt_laborer`, Deimos), killed the four, loaded: `1 moved back, 4 respawned`, `4 newer bodies cleared`, floor `12 cleared, 9 of 9 put back`, `3 of your 3 weapons reset`, no exception. The owner: "looked better this time". Drift check: two characters 2.2 and 2.8 m off, both in Combat (fighting).
- Not reset: blood. Story and arena loads never touched `Decay_Manager.decayList` (debris, casings, holes, splats); the Playground load clears them through `InteractiveMode.ClearAll_DoIt`.
- Fix in 0.7.4: a save remembers the newest piece of each list while the level stays loaded; a story or arena load removes everything after it, or all of it when the mark is gone. Built clean, deployed; not yet played.
- led to: none

### T-20261022-7 · 2026-10-22 · live use by the owner · owner's PC, beta 2.20.a · 0/1 (enemies, weapons and corpses on load)
- live-f11-bodies · fail · Director 0.7.2 in The Boom Factory, "Room2 - flame trap demo": saved "fight 2" (two `Merc0_Grunt_laborer`, Deimos), loaded twice (`3 moved back`, then `1 moved back, 2 respawned`). The owner saw an enemy missing its weapon and one in the wrong place, and the dead grunts' bodies stayed beside their respawned copies. Log: `scene: no item card N_Crate` on both loads.
- Causes: `RestoreLevel` respawns the dead but never removed their old bodies; `N_Crate` (the laborer's crate) is a card outside `Stats/Weapon/<category>/`, which `StatCard_Base.RegisterAllUnmoddedCardCatalogs` never registers, so `ReturnCardByName` finds nothing after `Strip` despawned the original; `TeleportTo` completes only one combat order, so a move order given after the save takes the enemy off its spot (likely; the log could not show it).
- Fix in 0.7.3: scene files version 4 list the bodies lying at save time; a load clears the old bodies of the respawned and every newer body (Clear corpses' keep rules); unregistered item cards fall back to `Resources.FindObjectsOfTypeAll<StatCard_Item>()`; `Put` clears non-persistent Combat and Idle move orders before the teleport; one log line per character (how matched, metres moved, weapons given or failed) and a 2 s drift check. Built clean, loads in game; not yet played.
- led to: none

### T-20261022-6 · 2026-10-22 · live use by the owner · owner's PC, beta 2.20.a · 0/1 (thrown weapon on load)
- live-f11-thrown · fail · Director 0.7.1 in The Boom Factory: saved "test alpha" holding R_Shot_ChopPump5, threw it, loaded twice: the shotgun disappeared. Log: `13 cleared, 12 of 12 put back, ammo reset in 2 of your 3 weapons`: the thrown shotgun (not career-registered) was cleared as a loose floor weapon, and `RefillAmmo` only touched weapons still in their slot.
- Fix in 0.7.2: `RefillAmmo` picks the same weapon back up (`ClearItemFromSlot` on a dead owner, `Combatant_Base.PickupItem(item, hand, slot)`), or gives a fresh one if none is left, and runs before `RestoreFloor`. Built clean and deployed while the game was running (takes effect on restart); not yet played.
- led to: none

### T-20261022-5 · 2026-10-22 · live use by the owner's family · owner's PC, beta 2.20.a · 0/1 (ammo reset)
- live-f11-ammo · fail · Director 0.7.0 in The Industrial Sector and The Boom Factory: three saves and loads, no exception; floor weapons cleared and put back (5 of 5, 10 of 10, 10 of 10), but the player's shotgun ammo did not reset. Cause: the player's loadout was saved only in the Playground (`"playerItems": []` in the file) and Deimos's, though saved, was skipped as a career-weapon holder.
- Fix in 0.7.1: player loadout saved everywhere; outside the Playground, and for career-weapon holders, `RefillAmmo` sets the saved ammo on the weapon in the same slot with the same card (`CancelReload`, `ResetAmmo`, `SetAmmo`, `SetMags`) without swapping it. Built clean, loads in game; not yet played.
- led to: none

### T-20261022-4 · 2026-10-22 · live use by the owner's family · owner's PC, beta 2.20.a · 0/1 (F11 load resets weapons)
- live-f11-weapons · fail · Director 0.6.1: the player reported that loading a scene "doesn't reset the weapons, just the people". Cause: scenes saved only the hands and grenade of living NPCs, matched characters kept whatever they held ("weapons stay"), loose weapons were never saved, and the Playground's Clear All despawned floor weapons without putting any back.
- Fix in 0.7.0 (scene files version 3, `SceneItems.cs`): holsters, ammo, loose weapons and the Playground player's loadout saved and restored; career (`amRegistered`) and quest items skipped. Built clean, deployed, game loads it with no exception; not yet played.
- led to: modtools game-recipes line on loose weapons and loadouts.

### T-20261022-3 · 2026-10-22 · evergreen-tester subagents · dev-machine, game installed (build 25615196) · 1/1 (outcome-1 run 1, plus baseline)
- New expectation in outcome-1: patches applied once (the loader runs `PatchAll()` under the HarmonyID unless `ManualPatch` is true, so `Initialize()` must not call it).
- outcome-1 · pass · info.txt with AssemblyPath, HarmonyID, EntryType, `ManualPatch: false`; `ModEntry : IModEntrypoint` logs the HarmonyID and does not patch (comment cites the decompiled loader); csproj net48 with Private=false references from Managed, justified from 0Harmony's target; placeholder patch only; built clean first try; `mpn.py check-mod` clean.
- outcome-1 baseline (no skill) · pass · decompiled the loader itself, no PatchAll in Initialize, net48 after a failed net472 build (MSB3274, CS0246). The new expectation did not separate this time; the skill's edge is one build instead of two and fewer steps.
- led to: none

### T-20261022-2 · 2026-10-22 · live use by the owner's family · owner's PC, beta 2.20.a · 1/1 (F11 in a story level)
- live-f11-story · pass · Playground Director 0.5.1 in The Industrial Sector: saved "new save" (14 characters) and "save2" (12); loads: 13 moved back + 1 respawned, 14 moved back, 12 + 2 respawned, 12 + 1 removed; files hold `chars`; Player.log has no problem line. The owner: "it vaguely seemed to work".
- Not exercised: arenas, Playground scenes, F9 guard.
- led to: L-005 confirmed (helpful +1)

### T-20261022-1 · 2026-10-22 · live use by the owner's family · owner's PC, beta 2.20.a · 0/1 (F11 in a story level)
- live-f11-story · bug · fail · Playground Director 0.5.0 in Warehouse Raid: saves logged 1, 2 and 4 characters, every load said the saved room `""` was closed; the files under PlaygroundDirector/Scenes held no `playerRoom` and no `chars`. One save threw `IndexOutOfRangeException` in `Health_Manager.ReturnCharacterDead` via `Game.Alive`.
- Fixed in 0.5.1 (built, deployed, helper test 10/10 in the scratchpad); in-game recheck pending. F9 still never logged.
- led to: L-005

### T-20261021-1 · 2026-10-21 · live use by the owner's family · owner's PC, beta 2.20.a · 6/6 exercised
- Playground Director 0.4.0 (second mod built with this skill, recipes in references/game-recipes.md). The owner reports "everything worked". Player.log evidence: the 0.4.0 load line; `applied_harmony` lists the id (the new `RunControls_Attacks` prefix applied); F1 "7 characters removed"; F2 freeze and unfreeze three times; F3 squares shown and hidden 8 times; F4 listed 369 animations (8 own, 361 library) and played typing, mopping and talking loops; F10 search, attack, unaware; F5 to F8 swaps; no exception or problem line.
- Then played in the story mode: the owner reports everything works so far (no log read for it).
- Not exercised: F9 guard and F11 scenes (no log line, no scene file). Released anyway at the owner's request; confirm in the next report.
- led to: L-003 and L-004 confirmed (helpful +1 each)

### T-20261018-2 · 2026-10-18 · live use by the owner · owner's PC and a second PC, beta 2.20.a · 4/4
- Mod 0.2.0 playtest: F8 worked; F5 failed on characters mid-fight (NullReferenceException in `AI_TargetClaimer.SetPriority` from `Controller_Base.LostConnection`), so dying in a taken body could not be tried. Cause: duplicate claimants left by F6's hand-written `myKillTarget = null` (L-002).
- Mod 0.3.0 (claims released through the game before every swap, claimants pruned): the owner reports everything worked; Player.log shows 7 swaps (one back home) and no exception or problem line; `applied_harmony` lists the id. Installed from the public Workshop item 3822329165 on a second PC and worked there too.
- live-1 · edge case · pass · mid-fight take-over; live-2 · pass · arena and F8; live-3 · pass · squadless spawn dying returns home; live-4 · pass · Workshop install on another PC.
- Open, not fixed: the owner thinks one of two grunts added to the player's own team turned on the player once (unconfirmed, no log signal; the owner chose to leave it).
- led to: L-002, C-20261018-2

### T-20261018-1 · 2026-10-18 · live use by the owner · owner's PC, beta 2.20.a · 1/2
- First live use: a swap-control and make-everyone-unaware mod built with this skill (Step 5). Build, check-mod, deploy, enable and load passed (`applied_harmony` listed the id; the mod's Initialize line in Player.log; swaps worked).
- live-1 · edge case · fail · taking over a Playground spawn and dying threw NullReferenceException in `Controller_Player.SwapControllersWithSquadmate` from `KillMe`; and swapping into a locked arena from outside left no way back (room-scoped target pick). Fixed in mod 0.2.0 with a Prefix and a screen-space pick; confirmed in T-20261018-2.
- led to: L-001, C-20261018-1, R-20261018-1


### T-20261017-4 · 2026-10-17 · headless `claude -p` per run, plugin installed as madnesswright@mark-local · dev-machine · 12/12 (trigger and decoy, 3 runs each)
- First pass on 0.1.0: 30/36 across the plugin. Every miss was madnesswright-scenes: its four trigger runs went to the hub ("Start here") or to madnesswright-modtools ("my mobj doesn't load" caught "I built the sobj"). The runner also stopped at the first Skill call.
- After C-20261017-6 (routing lines in all three descriptions) and a runner that records up to three Skill calls: 36/36; the scenes skill fired first on all six of its runs; every decoy went to its sibling or to no skill.
- This skill: trigger-1, trigger-2 3/3 each; decoy-1 (boss room level) went to madnesswright-scenes or no skill 3/3; decoy-2 (RimWorld) no skill 3/3.
- led to: C-20261017-6

### T-20261017-3 · 2026-10-17 · evergreen-tester subagents · dev-machine · 3/4 then 4/4 (value cases, runs 2 and 3)
- action-1 · pass ×2 · listed Managed and decompiled with ilspycmd before any [HarmonyPatch]; target `Item_Ranged.WeaponReloadSpeed` quoted with file and line from the decompile; info.txt with AssemblyPath, HarmonyID, EntryType. Both runs first guessed the install folder as `Madness Project Nexus` and had to look again.
- outcome-1 run 2 · fail on the skill's text, pass on the evidence · the skill said net472; the build failed MSB3274/MSB3275 against the game's net48 0Harmony.dll, the tester switched to net48; the project's obj/ landed inside the mod folder.
- outcome-1 run 3 (after C-20261017-4) · pass · net48 built first try; project in FastReload.Source/, the mod held only info.txt and the DLL; no game method named.
- outcome-1 baseline (no skill, game installed) · mostly pass · read Assembly-CSharp metadata itself and found IModEntrypoint and the ModItem keys, but called PatchAll inside Initialize (the game already runs PatchAll unless ManualPatch, so it patches twice), invented `UnpatchSelf` once, and left AssemblyPath and EntryType semantics unverified. With the game on disk this case separates the skill only weakly.
- led to: C-20261017-4, C-20261017-5

### T-20261017-2 · 2026-10-17 · evergreen-tester subagents · dev-machine · 2/2 (value cases, run 1 of 3)
- action-1 · pass · looked for the install, `Managed/` and `Assembly-CSharp.dll` and for ilspycmd before writing code (the game's files were not downloaded); the HarmonyPatch target was written as a labelled placeholder; info.txt had AssemblyPath, HarmonyID, EntryType.
- action-1 baseline (no skill) · fail · no lookup of Assembly-CSharp or Managed; used BepInEx 5 (`BaseUnityPlugin`) instead of the game's own loader; no info.txt. The skill made the difference.
- outcome-1 · pass · csproj with Private=false references from MPN_MANAGED and a provisional netstandard2.0 with the rule in a comment; IModEntrypoint entry class logging the HarmonyID; no game method named; check-mod's only error was the expected missing DLL.
- Trigger and decoy cases not run. Owed.
- led to: none

### T-20261017-1 · 2026-10-17 · not yet run · skill · 0/0
- Suite scaffolded; no run recorded. Write the cases in `evals/evals.json` (at least two trigger prompts, two decoys, one action case with evidence, one outcome case), run the baseline without the skill, then run with it (`evergreen-test`).
- led to: none
