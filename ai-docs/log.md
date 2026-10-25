# Session log

Append-only, newest first.

## 2026-10-24 · session 15: two players played, Workshop 0.8.0, arena co-op prompt

- The owner and the requester played 0.8.0 with a pad and reported a pass. Player.log and the scene file agree (modtools T-20261024-3): player 2 joined, a story scene saved player two's Deimos (`"player":2`, version 6) and the load put it back with its weapons; no exception.
- Workshop item 3822329165 updated to 0.8.0 (the played DLL, byte-identical) on the owner's "release it". The description gained a "Two players" paragraph. `result OK`, visibility 0, 44 subscribers.
- A Workshop commenter asked for full arena multiplayer: both players start the arena themselves and pick their own origins. A research-first session prompt for it is in the owner's private notes (Owed 15).

## 2026-10-24 · session 14: Workshop 0.7.5, after-load fix 0.7.6, two players 0.8.0

- Workshop item 3822329165 updated to 0.7.5 as played, on the owner's OK (asked once; 0.7.6 changes only a log line, so it waits for the next upload). Description's F11 line names teams and animations; the "Made with madnesswright" line went live. `result OK`, visibility 0, 43 subscribers.
- 0.7.6 (PR #35, merged): the checks 2 s after a load remember the active scene and are skipped with one line when it changed. The 0.7.5 log showed the cause: the game loaded the overworld (`Story_Overworld_IndustrialSector`) before the check ran.
- Two players: the owner chose the game's own local co-op (one question, three options). Fresh decompile, same build 25615196. Findings in [notes/2026-10-24-two-player-research.md](notes/2026-10-24-two-player-research.md): pads join with Start into a free squadmate (or at the next door, where story stages add a bonus character); the Playground's Clear All removes every character but player one's. Plan and rulings: [plans/2026-10-24-two-player.md](plans/2026-10-24-two-player.md).
- 0.8.0: `Game.IsPlayer` and `OtherPlayers`; F2 and F9 skip and let go of any player's body; the left-click block only stops the keyboard player; F8 names the player holding your home body; scenes version 6 save the other players' characters (`player`), a load puts them back with weapons and never respawns or removes them; the Playground load clears the room itself, sparing every player; `PlayersFeature` logs joins, leaves and character changes. Clean build (0 warnings), deployed, DLL checked. Not launched or played: a play needs two people and a pad.

## 2026-10-24 · session 13: Director 0.7.5 played, teams and animations pass

- The owner played 0.7.5 on their PC (Playground and The Boom Factory). Player.log: 0 hostile squadmate pairs on every load; the Playground's posed agent played `anim_cookstation_stir` again (1 of 1); scene files are version 5 with `emote`. Drift on the story load 0 of 5 (0.7.4: 9 to 14 of 15). modtools T-20261024-2.
- Small bug seen: the 2 s after-load check still runs after the player leaves the level, and reports everyone gone. Not fixed yet.
- Workshop still 0.7.4; the upload of 0.7.5 waits on the owner's OK.

## 2026-10-24 · session 12: teammates fighting and lost animations after a scene load (Director 0.7.5)

- Report (the requester via the owner, Director 0.7.4 from the Workshop): after a load, characters that were mid-animation at save are no longer doing it, and teammates sometimes attack each other. No evidence on the owner's PC: newest Player.log and scene files are from 2026-10-22, so the play was elsewhere or its log was overwritten (modtools T-20261024-1).
- Same build 25615196; fresh decompile in the session scratchpad. Teams: `Combat_Manager.FactionIsAttackable` = factions differ (or attacker None) and squads differ. `Spawn` gave every respawned character a new `CreateSquad`, so a respawned member of a squad with survivors was a foe to them when the faction was None (every Playground spawn: `InteractiveMode.UnitSpawn_Done`). Story factions (Enemy, MERCs...) do not fight across squads. Also: the Playground's Clear All empties only the current room, so saved characters standing in other rooms were doubled into a new squad.
- Animations: `PlayAsEmote` stores no clip; it lives in `myAnimator_Character.myOverride` under `Emote_0` or `Emote_1`, chosen by the private `emoteState` (flipped after each play).
- 0.7.5: squads are seeded from the characters put back (same character first) and respawns join them; a put-back character in a squad that would fight its saved squadmates joins theirs (`Rejoin`; never your squad or essential ones; wave units rejoin the running wave). Playground loads put back saved characters still alive in other rooms by id. Load lines name the squad and faction now and at save; 2 s later Player.log lists saved squadmates now hostile. Scene files version 5 add `emote` (looping clip name); 0.5 s after a load `Repose` replays it with `IdleAnimFeature.Pose` for characters still unaware (frozen ones released, posed, refrozen). Ruling: one-off moves are not replayed.
- Tests: clean build (0 warnings), deployed to the owner's Mods folder (DLL checked). Not launched or played; not on the Workshop.

## 2026-10-23 · books track 9: enemy roles, waves, level plan, attack matrix

- From Rogers, *Level Up!* (Ai/books/plugin-learnings, track 9). encounter-design.md: roles, orchestration order, wave pacing, finale, fairness, playtest items 9 and 10; new level-plan-template.md linked from Step 4; madobjects.md attack matrix. Rulings M1-M6 in [decisions/2026-10-23-books-track9.md](decisions/2026-10-23-books-track9.md).
- The enemy cards are inside compiled bundles, so roles are decided per card in play (M1).
- Evals (headless `claude -p`, branch via `--plugin-dir`): scenes outcome-2 0/3, then 1/3, then 3/3 after a finale rule and limits in the column headers (scenes L-002); modtools outcome-3 3/3; both baselines fail.
- Budget: icon 1024 to 512 px, tracked repository 1,500 to 1,055 KB.

## 2026-10-22 · books track 7: Blender FBX settings

- From the Blender books (Ai/books/plugin-learnings, track 7). Probed on Blender 5.2.1: two actions on an armature exported with `bake_anim_use_all_actions=True` re-import as `Rig|Rig|walk` and `Rig|Rig|swing`; `add_leaf_bones=True` turns 2 bones into 3. polywright's recipe `rigify-export` (polywright PR, same track) shows 278 of 706 Rigify bones survive a deform-only export.
- madobjects.md gained "From Blender"; SKILL.md Step 2 item 3 points at it. New eval outcome-2: skill 3/3, baseline fail (T-20261022-10). Unity 2019.2 is not installed here, so clip names and the axis mapping stay untested.
- Budget: tracked repository 1,496 KB of the 1,536 KB yellow line before this entry; the next additions need a trim.

## 2026-10-22 · session 11d: plugin v0.1.8

- The owner said yes to the tag: PR #27 (plugin.json 0.1.8, Unreleased changelog plus a 0.7.4 line became 0.1.8), tag v0.1.8, release workflow green, `madnesswright-0.1.8.zip` (108,641 bytes). Local install updated 0.1.7 to 0.1.8. The wiki still names 0.1.7 (update with wikiwright when convenient).

## 2026-10-22 · session 11c: 0.7.4 played, on the Workshop

- Play (the owner, The Industrial Sector, scene "outside", 15 characters): four loads, no exception; each cleared the newer mess (56, 66, 89, 275 pieces) and bodies (3, 3, 4, 7), put 8 of 8 floor weapons back and reset 3 of 3 of your weapons; the last load after F1 respawned 8. The owner: "Looks good" (modtools T-20261022-9).
- Still open: the drift check found 9 to 14 of 15 characters off their spot 2 s after each load, Unaware ones 2.4 to 2.7 m, Combat ones up to 15 m (running in 2 s). The owner did not see a problem; look at it if a report names it.
- Workshop item 3822329165 updated to 0.7.4 on the owner's word ("release the mod to steam"): description's F11 line now names ammo, thrown weapons and the clearing of bodies, blood, holes and casings. `result OK`, visibility 0, 35 subscribers, tags `scripts`.

## 2026-10-22 · session 11b: blood on scene load (Director 0.7.4)

- First play of 0.7.3 (the owner, The Boom Factory, scene "ambush"): "looked better this time, but the blood wasn't reset". Log: 5 saved, 4 laborers respawned, Deimos put back (same character, moved 9.2 m), `4 newer bodies cleared`, floor 12 cleared and 9 of 9 put back, 3 of 3 of your weapons reset, no exception. 2 s later two characters 2.2 and 2.8 m off their spot, both in Combat (fighting, not drift). Scene file version 4 with `"dead":""` (no bodies at save time). Bodies, weapons and placement pass (modtools T-20261022-8).
- Cause: story and arena loads never touched the mess. `Decay_Manager.decayList` (private static, 5 capped lists, oldest first: 0 bodies, 1 debris, 2 casings, 3 holes, 4 splats; caps 8/30/30/50/250). The Playground load already empties them through `InteractiveMode.ClearAll_DoIt`.
- 0.7.4 (`SceneMess.cs`): a save remembers in memory the newest object of lists 1 to 4 (keyed by stage key and save time); a story or arena load destroys every entry after it, or the whole list when the mark is missing, destroyed or decayed (then everything left is newer than the save). No file format change, so older scene files get it too. Load line adds the count.
- Small ruling (flagged in the mod README): bullet holes, casings and debris go with the blood, as the menu's clear button groups them; blood from before the save is not painted back after a restart.
- Tests: clean build (0 warnings), deployed. Not played.

## 2026-10-22 · session 11: 0.7.3 still unplayed; code reading of the never-exercised paths

- The owner closed the game session 10 had left running. Its Player.log (backed up as `Player-20261022-fight3.log` in the owner's backup folder) shows `Director 0.7.3 loaded` and the main menu only: no level, no scene file newer than "fight 2". 0.7.3 is still unplayed (Owed 12).
- Read the paths no play has reached against a fresh decompile (same build 25615196), without changing code:
  - **INSERT between arena waves:** the game's `CanPause()` is false while `Arena_Manager.WaveCompleted()`, but the Director menu calls `Game_Manager.PauseGame` itself, so INSERT opens between waves (Clear corpses there should work). It also means "Save a scene" from the menu works between waves, which the README says is impossible. A scene loaded between waves does not rewind the wave counter. Check in the arena play, then fix the README line or block it.
  - **ESC closing a Director window also opens the game's pause menu** when the game can pause (`UI_Keystrokes.Update` reads the same ESC press; `Event.Use` does not stop it). Expected; press ESC again.
  - **F9 guard** sets `wanderingDisabled`, `SetFrozen(true)` and Unaware, like the spawner's `FixedInPlace`, but not the spawner's `noRagdollInCombat = true`: a guard hit in a fight may ragdoll off its spot. Add it (saved and restored) if the guard play shows that.
  - **Clear corpses and level completion:** story rooms finish through `Squad_Main.ReturnFactionStillAlive` (skips despawned members) and `Check_CharacterDies` counts at death, so clearing corpses cannot stop a level finishing. `DeSpawn` drops the bodies' weapons to the floor, as the console's clear does.
  - Pause sources are a `HashSet`, so the menu and scene window opening each other cannot leave the game stuck paused.

## 2026-10-22 · session 10: bodies, crate and drift on scene load (Director 0.7.3)

- Report (the owner, Director 0.7.2, The Boom Factory "fight 2"): the load "didn't properly reset all the enemies and their weapons", and corpses stayed. Asked one question: what looked wrong was a missing weapon and an enemy in the wrong place (not health, not alert). Log: `3 moved back`, then `1 moved back, 2 respawned`, and `scene: no item card N_Crate` both times (modtools T-20261022-7).
- Same build 25615196; fresh decompile in the scratchpad. Causes: respawned characters left their old bodies; `N_Crate` is outside the folders `RegisterAllUnmoddedCardCatalogs` registers (`Stats/Weapon/<WeaponCategories>/`), so after `Strip` despawned the crate nothing could make a new one; `TeleportTo` completes only one combat order, so a move order given after the save can walk an enemy off its spot (the likely "wrong place"; the log could not prove it).
- 0.7.3: scene files version 4 add `dead` (unique ids of bodies at save time, joined by `|`, a flat string so `JsonUtility` keeps it). `RestoreLevel` then clears, in open rooms, the old bodies of the respawned and any body not in `dead`, keeping what Clear corpses keeps. `Generate` falls back to `Resources.FindObjectsOfTypeAll<StatCard_Item>()` (cached per level). `Put` runs `Orders_Manager.ClearMoveOrders(c, Combat/Idle, true)` before teleporting (standing orders stay). New log lines: one per saved character (how matched, metres moved, weapons given or `(failed)`), and 2 s after the load each character more than 1.5 m off its spot. Load line adds the body count.
- Small rulings (flagged in the mod README): bodies from before the save stay; a version 3 file clears only the respawned characters' old bodies; walk orders from after the save are dropped.
- Tests: clean build (0 warnings), deployed, game launched by the session: `Director 0.7.3 loaded`, no exception. Left running for the owner's play. Not played; not on the Workshop.

## 2026-10-22 · session 9c: thrown weapon vanished on load (Director 0.7.2)

- Report (the owner): in a story level, threw the shotgun, loaded, and the shotgun disappeared. Log: the load cleared 13 floor weapons and put back the 12 saved ones; the extra was the thrown shotgun (its file record shows it in the player's hand, so it was not on the floor list), and the ammo refill skipped the empty hand (modtools T-20261022-6).
- 0.7.2: for an emptied slot, `RefillAmmo` takes the nearest weapon with that card that no living character holds (career weapons included), clears it from a dead owner's slot, `PickupItem`s it into the saved hand or holster, then sets the ammo; if none is left it gives a fresh one. The player's refill now runs before `RestoreFloor`. Built clean, deployed while the game ran (restart needed); not played.

## 2026-10-22 · session 9b: ammo reset (Director 0.7.1)

- Report (the owner): "the ammo in the gun didn't reset". Player.log: three story saves and loads with 0.7.0, no exception, floor weapons restored; the "Shotgun" scene file had `"playerItems": []`. Session 9's ruling saved and reset the player's weapons only in the Playground, and squadmates with career weapons were skipped whole (modtools T-20261022-5).
- 0.7.1: the player's loadout is saved in every mode; outside the Playground (and for anyone holding an `amRegistered` weapon) only the ammo goes back, onto the weapon in the same slot with the same card, so career weapons are never swapped. `ApplyAmmo` now calls the owner's `CancelReload` and `Item_Ranged.ResetAmmo` first: a reload in progress would otherwise `LoadAmmo` its limbo rounds over the saved count. Clean build, deployed, loads in game. Scenes saved with 0.7.0 in story mode must be saved again.

## 2026-10-22 · session 9: scenes reset weapons and items (Director 0.7.0)

- Report (the owner, for the requester): loading a scene "doesn't reset the weapons, just the people", and they want weapons and items reset too.
- Same build 25615196; fresh decompile in the session scratchpad. Cause: `Capture` saved only living NPCs' hands and grenade, `Put` left matched characters' weapons as they were, nothing saved loose weapons, and in the Playground `InteractiveMode.ClearAll_DoIt` despawns the room's loose `Item_Held` without putting any back (modtools T-20261022-4).
- Built 0.7.0: scene files version 3 (`SceneItems.cs`, `SceneData` now partial). Saves each character's hands (slot 0), holsters (slot 1) and grenade with ammo (`Item_Ranged.myAmmo + myAmmoLimbo`, `myExtraMags`; melee `myMeleeAmmo`), every loose `Item_Held` (no living owner) with position, rotation and room, and the player's loadout in the Playground only. Load: characters are stripped and re-given their loadout, then loose weapons in open rooms are `DeSpawn`ed and the saved ones laid back with `Item_Held.CreateHeld` (last, so bodies removed by the load lose theirs too).
- Rulings (small, flagged in the mod README): your own weapons reset only in the Playground (a story or arena career owns them); anyone holding an `amRegistered` career weapon keeps its loadout; quest items untouched; rooms the game has locked keep their weapons; older files load as before and the load line says to save again.
- Tests: clean build (0 warnings), deployed, game launched: `Director 0.7.0 loaded`, no exception, closed by PID. No in-game save or load yet. Not on the Workshop.

## 2026-10-22 · session 8b: renamed Director (0.6.1)

- Request (the owner): rename to something like just Director and make clear it is not Playground-focused. Display name `Director (all game modes)`, Workshop title `Director (was Playground Director)` (kept the old name in brackets so the 32 subscribers recognise it), Playground tag dropped, descriptions and READMEs lead with "every game mode". Folder, DLL and HarmonyID unchanged. PR #18, Workshop updated (`result OK`, visibility 0, tags `scripts`), wiki commit 15ba40f.

## 2026-10-22 · session 8: Director menu and clear corpses (Playground Director 0.6.0), v0.1.7

- Request (the owner, for the requester): a button to clear corpses, and a menu of options that can grow. Same build 25615196; fresh decompile in the session scratchpad.
- Corpses: every non-essential death is queued in `Decay_Manager` (`Condemn_Character` at the end of `KillMe`), but `ClearAll_Character` only `Object.Destroy`s that queue, with no `DeSpawn`. The developer console's clear (Dev_Control, minus key) walks each room for `HaveRunKillMe` and calls `Decay_Manager.DestroyMe_Character`, which arenas also use; the mod does the same over every combatant and keeps your body, your home body, your squad and playable characters, bosses, cutscene actors, serial-numbered (scripted) bodies, `amEssential` ones and bodies dead under 45 frames (`ReturnAppearDeadAndInert`). Blood and debris: `ClearAll_Splats`, `_Holes`, `_Casing`, `_Debris`, a second button.
- Menu: INSERT (`decomp.py keys`: read only in `ModTestTools`, -devtools only; also free: BackQuote, Minus, Equals, M, Pause, ScrollLock). Works paused or not (own pause source). Registry of `MenuEntry` (label, key, modes, pick, action); keyed lines queue a one-frame key press that `Feature.Pressed` reads; scenes open straight from the menu because they need the pause. No key for the two clear actions.
- Tests: clean build; deployed; launched the game: `Playground Director 0.6.0 loaded`, main menu reached, no exception. No in-game use yet.
- Shipped on the owner's go-ahead ("release everything"): PR #15 merged, v0.1.7 tagged and released, plugin reinstalled locally (0.1.5 to 0.1.7). Workshop item 3822329165 updated to 0.6.0 after the owner chose "upload now" over waiting for the playtest: `result OK`, visibility 0, 32 subscribers.
- Wiki updated to 0.1.7 by a subagent with wikiwright (9 pages, live check clean; verify outputs re-run on Windows and WSL and saved over the old baselines).
- Gotchas met:
  - Auto mode refused copying the uploader zip's DLLs into `lib/` until the owner approved in the session.
  - `wsup.csproj` copied `steam_api64.dll` to `out/lib/`, so `SteamAPI_Init` threw `DllNotFoundException`; fixed with `Link="steam_api64.dll"`.
  - With relative paths the upload reached 100% and then said `result FileNotFound`; absolute Windows paths worked.
- Budget left yellow (see HANDOFF Owed 6).

## 2026-10-22 · session 8: eval fixes

- Owed 3: `Arena_PG` (and its .meta) moved from `tests/fixtures/scene-project` to a new `tests/fixtures/arena-project` with a copy of the prefab Library and TagManager and no reference scene; scenes action-1 points there. Skill pass; the baseline still found S01 and S05 by matching prefab GUIDs to the Library and reading Room_Layout's children (scenes T-20261022-1). Without the Library, scene-lint cannot resolve the prefabs either, so the case now separates on cost and next steps only.
- Owed 4: modtools outcome-1 gained "patches applied once" (the loader runs `PatchAll()` unless `ManualPatch` is true). Skill and baseline both passed; the baseline decompiled the loader itself and needed a net472 build failure before net48 (modtools T-20261022-3).
- Owed 5: `evergreen.py eval-export` works, but its default folder `skills/<x>/evals/cases` is refused by `claude plugin eval` ("--eval-dir must not be inside the plugin's skills/ directory"); `--out evals/<skill>` in a scratch copy of the plugin worked. Scenes suite (2.1.281, sonnet judge, $5.01): triggers 3/3 with, 0/3 without; decoys quiet; action-1 had no shell (granting Bash is refused on native Windows: "the Windows sandbox is not active ... feature gate off") and outcome-1 had no Write tool, so both failed in both arms. `worth --record`: UNPROVEN, in scenes' evergreen.json. Not tried for the hub or modtools; next is `worth --ab` (headless `claude -p`) or WSL2. The case folders were not committed.
- Recorded with `evergreen.py tested` for scenes and modtools. No version bump, no tag.

## 2026-10-22 · session 7: story-mode load fix (Playground Director 0.5.1)

- Second play (The Industrial Sector): 0.5.1 saved 14 and 12 characters and loaded them back four times (respawns and one removal), no exception (modtools T-20261022-2). v0.1.6 tagged and released on the owner's go-ahead.
- New request: a menu of Director options that can grow, with a clear-corpses action (HANDOFF Owed 11).

- Report: the owner said load "didn't seem to work right in story mode". Player.log (Warehouse Raid): saves logged 1, 2 and 4 characters, but every load said `"X" was saved in room , which the game has closed`; one save threw `IndexOutOfRangeException` in `Health_Manager.ReturnCharacterDead` from `Game.Alive`.
- Cause 1: the scene files held only the flat fields; `JsonUtility` dropped `playerRoom` and `chars` (types from the runtime-loaded mod assembly). F11 never saved characters in 0.4.0 either. Fix: `SceneData.ToJson`/`FromJson` serialize each flat record with `JsonUtility` and join and split the nesting; a file with no room and no characters says "save it again".
- Cause 2: `AmDead()` reads `myCorpusTrack[0]`; a combatant not yet set up has an empty track. `Game.Alive` checks it first.
- Tests (T-20261022-1): build clean; a scratchpad test of the join and split helpers passed 10/10, including quotes and brackets inside strings and the real broken `NEWSAVE.json`. 0.5.1 deployed to the Mods folder; in-game check pending. modtools L-005 and the game-recipes line corrected (it claimed JsonUtility handled nested classes).
- The two old scene files (`Test.MP4`, `NEWSAVE`) are empty and stay; loading them now says to save again.

## 2026-10-21 · session 6: scenes in every level (Playground Director 0.5.0)

- Request: the requester wants F11 scenes to save and load in the story levels and arenas, not only the Playground; the owner approved a deeper rework.
- Same build (25615196). Save backup before work. Player.log from the last play had no F9 or F11 use and no scene folder: both still unconfirmed.
- Fresh decompile; two research agents in parallel (level and room identity, stage loading, career save, respawn; pause, Clear All, scripted characters, arena waves, skip flags). Findings: notes/2026-10-21-scenes-in-levels-research.md.
- Decision (decisions/2026-10-21-scenes-in-every-level.md): A and B everywhere, C only in the Playground (starting a story stage writes the career save), no D. Loading outside the Playground matches existing characters to saved ones and puts them back instead of Clear All plus respawn; respawns come before removals so no faction-dead trigger fires.
- Built 0.5.0: `SceneData.cs` split out (version 2 files with stage key, rooms, serials, your place), `SceneFeature.cs` gains a pending load (wait for a Playground stage to load, or 0.75 s for a room's own characters). Clean build, deployed; playtest pending.
- Gotcha: `Game_Manager.ClearPauseGame()` on every scene load wipes the mod's pause source; the window now closes on `SceneManager.sceneLoaded`.

## 2026-10-21 · session 5: Playground Director (swap mod 0.4.0)

- Release: the owner reported "everything worked"; Player.log confirmed F1 to F4, F10 and F5 to F8, no exception, but no F9 or F11 use (released at the owner's request; noted). New preview (Z-Image Turbo, seed 4102, four candidates across both ComfyUI hosts). Workshop update of 3822329165 through session 4's uploader build (same Program.cs): `result OK`, visibility 0, title changed, 28 subscribers. Tagged v0.1.5 on the owner's go-ahead.

- Request: the swap mod's player sent eight new keys. Decided to grow the existing mod (one Workshop item he already has, F5 to F8 kept) under the name Playground Director, split into one `Feature` component per key group so one failing cannot stop the rest.
- Fresh decompile (same build 25615196). Three research agents (freeze/kill/AI, idle animations and movement, Playground spawn and saving) ran in parallel on the scratch decompile; findings in notes/2026-10-21-director-mod-research.md.
- Keys: a grep found seven of the requested keys in game code; reading the callers showed all were debug-only, `-devtools`-only or profile-window-only (L-003). Only CapsLock (autowalk) is live in play.
- Built F1 (RemoveFromGame on non-playable characters), F2 (controller lock, waypoints cleared, SetFrozen, animator speed 0), F3 (IMGUI squares, AssignWalkToPosition, Harmony prefix skipping RunControls_Attacks while placing), F4 (PlayAsEmote list), F9 (FixedInPlace copy), F10 (SetAlertStatus cycle, SetKillTarget), F11 (JsonUtility scenes outside Mods/, own pause source). Builds clean, deployed; playtest pending.
- New `decomp.py` saved reading whole decompiled files: `find` returned each signature in one line for about 60 members.
- Gotchas met:
  - The research agent suggested saving under `Mods/<mod>/`: the loader would read those JSON files as cards (L-004).
  - A Bash heredoc with a Windows path in doubled backslashes is refused by the hook; write paths with forward slashes.
  - `find` capped at `--max 3` hid `Char_Data.CreateCharacter` behind three `Char_Background` matches; raise `--max` before concluding a member is missing.

## 2026-10-18 · session 4: swap mod 0.3.0, example in the repo, Workshop prep

- 0.2.0's F5 failed on characters mid-fight. Cause (fresh decompile, beta v2.20.a): F6 nulled `myKillTarget` by hand, leaving a stale entry in the target's `AI_TargetClaimer.myClaimants`; the AI re-claimed and `ClaimTarget` adds duplicates; `Controller_Base.LostConnection` nulls `myOwner`, removes one copy, re-scores the rest and throws in `SetPriority`. Not team-based. 0.3.0 releases both controllers' claims before a swap, prunes dead and duplicate claimants, F6 unclaims through the game, and a swap that still throws reconnects orphaned controllers. Deployed; the owner's playtest passed on two PCs (T-20261018-2).
- The mod is now `examples/SwapUnaware` (PR #2, merged, plugin 0.1.4, not tagged). The repo copy is the build folder; `mpn-mods/SwapUnaware` keeps old zips and a MOVED.md.
- Workshop: no in-game uploader (decompile only queries). Community MPN-Workshop-Uploader moved to github.com/placenamehereig/MPN-Workshop-Uploader (MIT, 1.1.0, Facepunch.Steamworks, interactive console; its content path must contain a dot, so name the folder `X.content`). The owner wants the item Public ("team" in the session-3 prompt was a misreading: he meant swapping characters). Uploaded as public item 3822329165 with a no-prompt Facepunch program, now examples/WorkshopUpload.

## 2026-10-18 · session 3: first beta launch, swap-and-unaware mod

- Beta launch: build 25615196, Beta v2.20.a, `mods_allowed: true`. GreenBloodHank enabled from the gear icon, `mods.log` showed `OVERRIDE: Hank (Character)`, and the owner saw green blood in the Playground. Real log shapes: [notes/2026-10-18-beta-launch.md](notes/2026-10-18-beta-launch.md).
- The owner could not find the gear icon before: the release build destroys the mod button (`Object_Modding_Remover`). No gear icon means not on the beta.
- Swap-and-unaware mod built (modtools' first live test): F5 swap to the character under the mouse (allies or enemies, as the requester asked), F6 everyone else unaware, F7 keep them unaware. No Harmony patch needed; a MonoBehaviour from `Initialize()` polls the keys. Source outside this repo; a pack script makes a zip with an `install.cmd` for another computer. Waiting on the in-game test.

## 2026-10-17 · session 2: real game, evals, install, launch

- Request: the owner's session-2 prompt (real-game checks, harder evals, local install, public launch).
- Real game: release branch, v1.11.a, build 25615280, Unity 2019.2.6f1, Harmony 2.4.2 shipped, no netstandard.dll. Decompiled Assembly-CSharp with ilspycmd into the session scratch folder (never into the repo) and read the loader: [notes/2026-10-17-real-game-loader.md](notes/2026-10-17-real-game-loader.md). The official doc's `mods.txt` is really `Mods/mods.log`.
- Branch not switched: backup taken, test mod GreenBloodHank deployed; waits for the owner.
- mpn.py: where/log read the build from Player.log, enabled.txt, mods.log line shapes; check-mod follows the loader's rules and flags near-miss Roots. 26 unit tests.
- Evals: new fixtures subtle-mod and Arena_PG; value runs 2 and 3 and baselines; triggers 30/36, then 36/36 after routing the level prompts to madnesswright-scenes (scenes L-001). A tester found net472 does not build against the game's net48 Harmony.
- Launch: public repo, v0.1.1 and v0.1.2 releases, wiki (10 pages, live check clean), banner, icon, Dependabot. Listings skipped: the owner halted awesome-copilot for good and put the Claude directory on hold.
- Gotchas met:
  - `cat > file` with no input inside a Bash call hangs until the 120 s timeout; write scripts with the Write tool.
  - The installed plugin is a cached copy: bump the version and `claude plugin update`, or headless trigger runs test the old descriptions.
  - A trigger runner that stops at the first Skill call scores "hub first, then specialist" as a miss; record up to three.
  - The vault clone committed with the Gmail address and the push was declined; set the no-reply email per clone.

## 2026-10-17 · creation

- Request: an evergreen plugin for modding MADNESS: Project Nexus and level editing.
- The game was not installed on the dev machine at the start; a Steam manifest for it (0 bytes downloaded) appeared during the session. Everything was built from research plus the developer's public tool repositories.
- Key discovery: no in-game level editor. Levels come from the official SceneTools Unity 2019.2.6f1 project (`.sobj`), assets from ModTools (`.mobj`), data from MadCards, code from built-in Harmony 2.4.2. Modding is releasebeta-only.
- Decisions: [decisions/2026-10-17-name-and-shape.md](decisions/2026-10-17-name-and-shape.md).
- Gotchas met:
  - The official Google Docs examples use curly quotes and a non-JSON colour syntax; `check-mod` flags curly quotes.
  - Google Docs fetch: `/export?format=txt` works where the normal view does not.
  - The Steam News API is the real changelog; the store news page renders empty to fetchers.
  - The SceneTools build menu says Compressed is *RECOMMENDED* while the 2024 doc says compressed loads slower; the skill offers both.
  - `rm -rf "$VAR/..."` is refused by a safety check; write `"${VAR:?}/..."`.
  - Heredocs holding a doubled backslash are refused by a hook; write generator scripts with the Write tool.
- Tests: 18 unit tests pass; six value cases passed run 1 with the skill; baselines for the hub and scenes action cases also found the faults (fixtures too easy), the modtools baseline used BepInEx and no info.txt (the skill made the difference).
## [2026-10-23] index | rebuilt (13 entries)
## [2026-10-24] index | rebuilt (15 entries)
## [2026-10-24] index | rebuilt (15 entries)
