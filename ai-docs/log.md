# Session log

Append-only, newest first.

## 2026-11-11 Nexus Prominence Workshop feedback pass

- Read the 14 Workshop comments, set the hostility aside, checked each technical claim in the generator (note: notes/2026-11-11-nexus-workshop-comments.md).
- Owner's play of build-v clean (mpn.py log: 0 problems in mods.log and Player.log); uploaded with change-notes-2026-11-11k.txt, result OK.
- Implemented all ideas (builds w, x, y). Dead end: a closed box for the camera-side wall faces the camera and blacks out the whole view (build-w); a thin slab with its renderer off works. The Madness terrain shader reads colour only from _MainTex and lights mostly from ambient, so editor shots look darker than Standard did.
- Skill lesson L-20261111-6, plugin 0.1.18.

## 2026-11-11 Nexus Prominence encounters and colour

- SceneTools facts used (MPN_SceneTools/Assets/Code/SceneObjs): `ProxyObj_Door.ProxyType` has spawner types Manhole, FloorGrate, Vent, VentDrop, VentSide, VentFan, Rapel; `Marker_Squad` (prefab Markers/Squad_Spawner) takes `SpawnFromDoors`, `SecondsInRoomTilSpawn`, `Wave` (WaveUnits with GenerationScale, MaxSquadSize, MaxUnitCount, WeaponStock pools such as NEXUS_T5, NEXUS_T7_Ranged); `ProxyObj_Togglable.ProxyType.Spikes`; `CustomAddon_ToggleAutomator` (TimeStayUp/Down in frames) whose header says to place it on a custom togglable.
- Spawn doors are added after ChainDoors: ChainDoors links the leftmost and rightmost door by x, so a spawn door added earlier could become an exit.
- Vent grilles on the back wall vanished twice: flush with the band and skirting (z), then dark on the near-black lower wall; now steel, 0.2 out from the wall.
- Build k: 40/40 corners, sobj 2.09 MB, check-mod clean, scene-lint 0 errors, deployed. Not played, not uploaded; notes workshop/change-notes-2026-11-11h.txt. F12 screenshots wait on the owner's play.

## 2026-11-11 Nexus Prominence juice: shootable set pieces

- Asked to use the game-juice plugin while staying true to the video. A Playground map has no game code, so juice lives in the set: SceneTools `CustomObj_Destructible` is converted by the game (`Converter_Proxy.ConvertCustomObj` -> `Interactive_Target`, decompile of build 25615196 in the session scratchpad) and swaps `DestructionStates` on hits, with `FlashColorOnHit`, FMOD hit/destroy sounds and an optional `Explosion` card. `Hazard_Splosive` also exists (game class with the same name) but no sample uses it, so it was not used.
- Added `Breakable()` and 23 breakables: room 2 case glass, room 7 four lamps (glow + cone), room 10 18 monitors. `AllBulletsPassThrough`, no bar, empty popup list, no explosion (constant `MonitorExplosion`, e.g. `ElecSplode`). Sounds used are ones the game's code or sample prefab name (`event:/Foley/Glass Bottles/Wine Bottle Break`, `.../Beer Bottle Break`, `event:/MechanicalFoley/Sparks/Electrocution`, `event:/Combat/Richochet/Richochet Sheild`); the strings bank is a compressed trie, so no full event list.
- Explosion cards (globalgamemanagers ResourceManager, UnityPy): barrelsplode, elecsplode, propanetank, nexusbomb, nexusburst small/med/large, grenade, shockblast and more under stats/explosion/.
- make_decals.py needs Pillow, which `python -I` hides; the Tex folder already had the decals, so it was skipped.
- Build: 23 CO_NX_ objects, Intact active and Broken inactive after SYNCH; NexusShots 40/40 corners OK (lab shots-h); sobj 1.96 MB; check-mod clean; deployed. Not played, not uploaded (Owed 24); notes in workshop/change-notes-2026-11-11g.txt.

## 2026-11-11 Nexus Prominence Workshop screenshots and video

- Workshop extra previews: images through `AddItemPreviewFile` (type 0), videos only as a YouTube id (`AddItemPreviewVideo`); Facepunch.Steamworks' Editor has no API for either, so wsup.exe gained a `previews` mode calling the steam_api64 flat API (SteamUGC_v014, SteamUtils_v009) after `SteamClient.Init(488860, false)`, then polling IsAPICallCompleted and reading SubmitItemUpdateResult_t (callback 3404, EResult first). Update without content keeps the files, title and main preview.
- Uploaded six editor renders and the film (v0suvMY3GgM). Result 1; page shows 6 screenshots and 1 movie.

## 2026-11-11 Nexus Prominence clean-up

- Owner: signs over doors and objects in rooms 1-2; clean up, match enemy weapons to the video, better Workshop image.
- Weapon cards: no list in the repo; UnityPy over resources.assets grouped MonoBehaviours by script class (StatCard_Ranged 84, StatCard_Weapon 101, StatCard_Character 111). Nexus cards: R_NN57 pistol, R_NM46 SMG, R_Nc762 rifle, R_Hrifle_NFAL, R_NS-V.10, R_NZucker, Revolver_Nex410_S, M_SwordNexus, Shield_RiotLight. Character cards' default loadouts were not decoded (no typetree); weapons are now set on the markers (`Held_R`/`Held_L`, `ClearDefaultWeapons`).
- Video frames (sheet_01): street guards with assault rifles, hall agents with pistols/SMGs, a riot shield, blades.
- Decal placement: first pass counted pilasters and seams as blocked and dropped nearly every sign; filtered thin and wall-spanning parts. Room 1's back wall really is full, so its signs got fixed mounts.
- Built, rendered (madnesswright-lab/nexus/rooms-2026-11-11e.jpg), 40/40 corners OK, SyncAndBuild, check-mod clean, deploy --force. Not played, not uploaded.

## 2026-11-11 · Nexus Prominence: camera reaches every corner, Madness dressing

- Owner's report: at the bottom left or right of room 1 the camera did not follow; the right exit barely showed. Cause (decompiled Cam_Main): the sample room's Leeway z is 0, so the camera never moves toward the front, and it stops about 5 units inside Border_Left/Right, which sat on the walls. New `FitCamera` in the generator: Leeway z 0.6 (position and look), side borders placed so the camera centre gets 10 units past a player 1.5 units from a wall (`EdgeLead`), Z borders on the floor edges.
- New `NexusShots.Render -corners`: reproduces Cam_Main's follow (zoom off, mouse centred) for a player at each floor corner, renders it with a red marker and logs OK/OUT. Before: 19 of 40 corners off screen (all rooms but the locker room). After: 40 of 40, player 0.22 to 0.31 in from the screen edge.
- Look pass against the frame sheets (owner: "more like a madness map, funnier, but true to the movie"): new tools/make_decals.py writes PNG signs and splats into the SceneTools project (out of the repo for size); `Gags` puts 3 to 5 floor blood splats, bullet-hole bursts and a wall splat in every room, red wall scrawls, the film's "Don't Blink" chalkboard in the spotlight corridor, and joke signs (Welcome to Nevada: population decreasing; Days without an incident: 0; Reward: yes; Now serving 9,999; Please do not shoot the receptionist (again); Fragile (like grunts); This side up upside down; Sniper nest, no peeking; Shutter closes on grunts; Lost & found; Who ate my lunch; Authorized personnel only (you are not); Self destruct do not press beside a big red button; a wet-floor sign).
- Built, SyncAndBuild (.sobj 1.97 MB), check-mod clean (no Author), deployed byte-identical. **Not played, not uploaded** (owner: don't publish until fixed and played). Description now links the film (https://www.youtube.com/watch?v=v0suvMY3GgM); change notes workshop/change-notes-2026-11-11c.txt. Renders: Ai/madnesswright-lab/nexus/rooms-2026-11-11d.jpg, corners-room1-2026-11-11d.jpg.

## 2026-11-10 · Nexus Prominence on the Workshop

- The owner tested the map ("looks good") and asked to publish it for the requester to test. Uploaded the deployed Mods/NexusProminence folder with examples/WorkshopUpload as new public item 3837705778, title "Nexus Prominence (Hank Raid map)", tag Playground, plain gray-and-red title-card preview (PIL, no video art) and description in examples/NexusProminence/workshop/. `result OK`. Next: the requester's test (Owed 24).

## 2026-11-10 (late): Nexus Prominence built and deployed

- Owner installed Unity 2019.2.6f1. Ran the generator headless (`-batchmode -executeMethod NexusProminenceBuilder.Build`): compiled first time, ten rooms written.
- SYNCH VISUALS has no menu item (inspector button, private `RoomObj_Editor.ActivateRoomVisuals`), so `NexusBatch.SyncAndBuild` calls it by reflection, saves, then calls `Build_Compressed`. Bundles land in LocalLow/Gibbing Tree/Madness Project Nexus SceneTools/SceneObjs.
- scene-lint: 0 errors; it reports `rooms: 1` because only room 1 stays a prefab instance (rooms 2-10 are unpacked copies; all 10 floors tagged). Warnings: S10 Camera_Point x9, S12 nine lights (from the sample room copies).
- Mod folder renamed `mod` -> `NexusProminence` (deploy names the Mods folder after it). check-mod clean bar no Author; deployed to Mods/NexusProminence. Owed 7 (headless SceneTools build) now proven.
- Not played: waits on the owner (enable in gear menu, Playground terminal, README checklist, then `mpn.py log`).

## 2026-11-02 (late): Director 0.17.1, empty INSERT menu

- A player got the INSERT menu to open with nothing but the header and Close (screenshot). No filter box showed, so the static `entries` list was empty, not filtered. Cause: `ModEntry.Unload` destroys the host and `DirectorMenu.OnDestroy` cleared the shared static list; on an unload then reload the old cleanup can land after the new features' `Register`. Fix: `current` instance guard on the clear, `Add` dedupes by label, empty-list message. Built 0 warnings, harness passed, deployed byte-identical; not played (no repro run in game). Merged PR #65, tag v0.1.14, wsup.exe updated item 3822329165 with change-notes-0.17.1.txt (result OK; GetPublishedFileDetails: Director, visibility 0, 56 subscribers, updated 01:19 UTC).

## 2026-11-02 · Director 0.17.0 on the Workshop

- The owner said to upload. Dist zip DLL matched build/; wsup.exe updated item 3822329165 with change-notes-0.17.0.txt (result OK). GetPublishedFileDetails: title Director, visibility 0, 55 subscribers, updated 17:41 UTC, description has the zed line.

## 2026-11-02 · Director 0.17.0: scenes keep one-shot animations and turned zeds

- The requester's asks: "save and load should save their temporary animations" and "a guy with clothes turned into a zed should be saved too".
- Animations: scene saves read only a looping emote from the animator (`IdleAnimFeature.CurrentEmote`), so an F4 clip that plays once was lost. `SceneMemory` keeps the F4 pick while the pose holds (forgotten by Stop or `EndPose`); the save falls back to it.
- Zeds (decompile): a zed's grab calls `PrepareForCoversion`; after death `AttemptConvertToStoredCharacter` runs `MorphCombatant(Char_Data.CreateCharacter(zedCard), keepScale, keepArmor, keepName, ...)`, so the new Char_Data is the zed card and the clothes stay only as meshes. `ZombifyMe` is an empty stub. `ZedTurnPatch` (Harmony prefix) notes the old card and `armorSlots` (slot|card) first; scene files version 8 save `zedFrom`/`zedArmor`; a respawn creates the old card, re-dresses it (`Catalog_Armor.GenerateArmor`), then morphs it the game's way. Colours are the card's (ruling).
- Built against the game's DLLs, harness `Memory()` check added, all repo checks pass, PR #62 merged, deployed byte-identical. Workshop upload refused by auto mode: handed to the owner (folder staged in the session scratchpad; next session re-unzip dist/SwapUnaware-0.17.0.zip). Plugin 0.1.13 tagged. Not played (Owed 23).

## 2026-10-31 · Director 0.16.0 on the Workshop

- The owner said to upload and publish. The 0.16.0 dist zip's DLL matched build/ and the deployed DLL; wsup.exe updated item 3822329165 with change-notes-0.16.0.txt (result OK).
- GetPublishedFileDetails: title Director, visibility 0, 55 subscribers, updated 2026-10-31 18:30:40 UTC, description has the props paragraph.
- Still not played (Owed 22). Plugin tag and wiki for 0.16.0 not done yet.

## 2026-10-31 · Director 0.16.0: props (clipboard, crates) in the weapon lists

- Ask: the requester says the game has a clipboard and box weapons and could not find them in the search or the pickers.
- Found: `resources.assets` holds `StatCard_Weapon` cards `N_Clipboard`, `N_Crate`, `N_CrateTech` (the box) and 25 other props under `stats/weapon/none/` (read with UnityPy from globalgamemanagers' ResourceManager container, in a scratch venv). `StatCard_Base`'s register loop does `if (value == WeaponCategories.None) continue;`, so `ReturnAllWeapons` and `ReturnCardByName` never see them; the 0.15.0 log line (282 weapons, 11 categories, no Other) agrees.
- Built: `WeaponSpawnFeature.Props()` loads the folder, entries go to category Props, Hidden; `WeaponSpawnFeature.Card(name)` falls back to the folder and replaces the weapon lookups in WaveFeature and SceneItems. Not registered with the game on purpose (Spawn_Randomizer, Market_Main, Arena_Manager read the register).
- Tests: build clean, DirectorFreezeChecks all pass (no new harness case: the change is Unity Resources calls). Deployed. Not played; some props may be inert in hand.

## 2026-10-31 · Director 0.15.0: filter boxes on long lists and a shared weapon search

- Request (the requester, via the owner): weapons are hard to find in long lists; a caseless wildcard filter box on lists over 20 items, and a weapon search reachable wherever a weapon is picked. The owner: no other faults or requests; nobody could play today.
- Lists found by reading the UI code: Spawn a weapon (categories, a category's weapons), the wave builder (enemies, weapon categories, weapons, throwables, saved waves), saved scenes, the Director menu, F4 idle animations. The arena guest picker is pad-only and was left alone. Saved waves showed only the first eight (`Take(8)`), so the 9th and later were unreachable: they get a filter past eight.
- Decompile (scratchpad; build 25615196): while a Director window holds the pause the game reads no player keys (`Combatant_Base.Update` returns before `Controller_Player.RunController`); `UI_Inventory` reads Tab only while open. The in-mod risk was Director's own: CAPS LOCK opened the scene window while paused, and \\, [, ], U, P, Delete fire while the F4 list (unpaused, controller locked) has its filter. Fix: `FilterBox.Eats(key)` is true for typing keys while a Director text box was drawn last frame and holds the keyboard; F keys and INSERT still work.
- Built: `ListFilter` (plain C#: threshold 20, matcher, `RecentPicks` of five) and `FilterBox`/`WeaponSearch` (IMGUI; owners stop drawing while it is open; a click acts after the layout closes). `WeaponEntry` gains Kind (thrown, ranged or melee by card type, presets by `LinkedWeapon`) and Stat (card `Damage`, `Ammo`, `MeleeDamage`). Harness `Filter()` written first and seen failing at "the box shows at the wrong length", then passing.
- Pack 0 warnings, check-mod clean, deployed byte-identical (UTF-16 `0.15.0` at offsets 0 and 1), test_mpn and test_decomp OK, CI green. PR #57 squash-merged; tag v0.1.12 released (`madnesswright-0.1.12.zip`, 121,552 bytes). Wiki: Versions row, Getting-Started hash, Home, footer, and the stale "updated there to 0.6.1" Workshop line. Workshop with the owner's OK: `result OK`, title Director, visibility 0, 55 subscribers, live description has the new paragraph.
- Rulings (mod README): wildcards also match anywhere; lists that had a filter keep it at any length; Recent is per game session, not saved; no wave or scene file version change.
- Not played (Owed 21).

## 2026-10-30 · Director 0.14.0: wave enemy limit and dual wielding switch, played and on the Workshop

- Owner: the requester tested 0.12.0 ("fine, nothing broke") and had seen 0.13.0. Requests (the requester): "I want the waves to have a limit on how many enemies there are to the wave"; "Dual wielding selection ... another thing in menu for dual wielding and can be enabled to have another one".
- Decompile (scratchpad; build 25615196): the game's own waves cap units alive at once (`WaveContainer.UnitCount_Max`, `Wave_Tandem.CanSpawnWithoutBreachingMax`, `spawnedUnitsStillAlive`), so the limit is read as alive at once. Not reused directly: its squads come from `Wave_Generator` and would lose per-enemy weapons; Director keeps its own queue (`WaveQueue`) with the same rule. `Char_Data.CreateCharacter` is data only (`GenerateCharacter` + `Init`), so the row can ask `ReturnCanDual` before spawning.
- Built: Max enemies alive at once (- and +, default no limit, total shown), Dual wielding off/on per row (off-hand picker only when on; the pick is kept when off), a refused-pair note on the row; wave files v3 (`# max alive: N` comment line plus a dual field; v2 reads as no limit, dual on where a 2nd weapon was set). Harness `WaveDual` and `WaveLimit` failed on a skeleton first, pass. PR #55 squash-merged; plugin 0.1.11 tagged.
- Played by the requester on the owner's PC (askwright set `director-0.14.0-play` in the private vault, 5 of 5 answered as guessed): never more than 2 alive, the next came after each kill until all 5, the dual enemy held two weapons, the single one held one, the menu is what they wanted. Player.log: six SPAWNs, each "at most 2 alive at once", "next in" lines up to 5 of 5, `Merc1_Zerker dual wields M_SwordFantasy`, no exceptions. Saved file `Test.txt` opened (L-005): v3 header, `# max alive: 2`, dual field 1 only on the Merczerker.
- Workshop: 0.14.0 uploaded with the owner's "release the mod" (`result OK`, title Director, visibility 0, 55 subscribers). The owner then noticed the description never mentioned dual wielding: 0.13.0 and 0.14.0 had shipped without description updates. Description fixed, re-uploaded, live text checked (dual wielding and the enemy limit present). New AGENTS.md rule: player-visible changes update `workshop-description.txt` in the same PR, and every upload checks the live description.
- askwright: the Check your answers screen showed a stray "null" (the DOM's `replaceChildren` writes null as text); fixed in askwright 0.7.1 (PR #23).

## 2026-10-30 · Director 0.13.0: second weapon and throwable in the wave builder

- Requests (the requester, via the owner): "some enemies need to be dual wielding weapons so can you add another weapon thing and it can be anything", and "sometimes I need an enemy to have both a weapon and has throwable weapon like hand grenades".
- No play before building (game not running). Mid-session the owner said the requester is testing the last version on their own PC and it is "good so far" (Workshop 0.12.0 at that point; no Player.log from that machine).
- Decompile (scratchpad only; build 25615196): characters hold `itemSlots[slot][hand]` (slot 0 in use, slot 1 stowed; hands 0 and 1). `Combatant_Character` checks `Char_Data.ReturnCanDual` (melee: one-handed plus DualMelee skill or HoldInOffHand; ranged: one-handed plus DualRanged/AdvancedDualRanged; DualWieldOverride trait allows all) and drops a refused off-hand item. `Wave_Generator` dual-spawns only with the SpawnDual trait. Grenade-slot cards (`AmGrenadeSlot`) go to `HoldThrowable`; `Controller_NPC.SetWeaponBasedModules` adds `AI_Modules.M_ThrowGrenade` whenever the character holds a throwable (cooldown from `myAmmoMax`), so the AI really throws. Thrown count is `Item_Ranged.myAmmo`; not exposed (card's stack).
- Rulings (mod README): 2nd weapon from the full list, off hand when ReturnCanDual allows the pair, else stowed backup (slot 1); Throwable picker of grenade-slot cards, no count, infinite ammo keeps it throwing; a grenade as 2nd weapon becomes the throwable unless one is picked; wave files version 2, 0.12.0 files load as none; weapon modules refreshed after spawn.
- Harness: `WaveSecondSlots` written first (failed at describe on a stub), passes. Pack 0 warnings, `--strict check-mod` clean, deployed byte-identical, DLL holds `0.13.0` and the new labels (UTF-16, odd alignment). PR #53 squash-merged, CI green. Workshop upload with the owner's OK: `result OK`, title Director, visibility 0, 55 subscribers, updated 2026-10-30 12:55 UTC. Not played (Owed 20).

## 2026-10-29 · Director 0.12.0: boss bar, wave builder, team change; Workshop title "Director"

- Requests (the requester, via the owner): "a button that who ever your mouse is hovering over has a boss bar"; "a button that allows you to set a wave of enemies with set guns and allows you to let a current enemy have infinite or normal amount and let a current enemy in the wave to have a boss bar". Extra objective from the owner's prompt: a key that changes the team of the character under the cursor. The owner also asked to rename the Workshop item to just "Director".
- No play since the 0.11.0 entry below (Player.log 2026-10-28 18:44); the requester was not around, so no play of 0.10.2 or 0.11.0 this session.
- Decompile (scratchpad only; build still 25615196): `UI_Game.AddBoss(Target_Object)` / `RemoveBoss` are public statics; `Controller_Boss.SetConnection` calls `AddBoss(combatant)`, automatons call it with `DisplayBossHealth`. Bars are `BossfightUI` (a `CharUI`) in `UI_Game.currentUI_Game.BossfightIcons`; only one is shown (`BossfightUI.currentVisible`), later ones show when a player hits them. Trap: `CharUI.LinkToCharacter` ends the target's existing `myHealthUI` (a player's or squadmate's HUD icon) and `EndMe` clears `myHealthUI` only when it is itself. The Playground's `InteractiveMode.UnitSpawn_Done` is the wave route: `Squad_Main.CreateSquad(Factions.None)`, `Wave_Generator.AssignAllDefaultWeapons`, a wave weapon near the level when none, `Spawn_Manager.SpawnCharacterToEntrance` at a random non-`disableSpawn` entrance of `Cam_Main.currentRoom` meeting `ReturnMeetVocationRequirements`, `AddMember`, alert Combat. Hostility: `Combat_Manager.FactionIsAttackable` (different faction, or attacker None, and a different squad); `Squad_Main.AddMember` moves faction and wave. Keys: `decomp.py keys` finds `[` and `]` unread anywhere.
- Rulings (mod README): ] boss bar on any living character, red flash, new bar shown at once, gone 2 s after death, not saved in scenes, HUD bar held aside and restored; Build a wave in the INSERT menu (characters with filter, per-row weapon / ammo / boss bar / copy / remove, spawn as the Playground does, near you when the room has no entrance, saved waves in `PlaygroundDirector/Waves/<name>.txt`); [ toggles enemy/ally relative to the keyboard player (allies to a squad of your faction or your own None squad; enemies back to their old squad or one shared Director None squad; players and career characters refused).
- Built as `BossBarFeature`, `WaveFeature` + plain `WaveRoster`, `TeamFeature`; `Game.CareerCharacter` split out of `Playable`. Harness: boss bar checks written first and failed on 0.11.0-style naive code at "adding it ended the player's HUD health bar", pass after the fix; wave roster and file checks; team checks (a mutant making a new enemy squad each time fails "two turned enemies fight each other"). Pack 0 warnings, `--strict check-mod` clean, deployed, DLL byte-identical with `0.12.0`, `boss bar`, `Build a wave` as UTF-16. check_skills, test_mpn, test_decomp pass; budget yellow (tracked 1,240 KB, installed skills 253 KB).
- Rename: info.txt Name "Director", mod README heading "Director", Workshop title to be plain "Director" with the next upload (folder, `PlaygroundDirector/` settings folder and HarmonyID unchanged).
- PR #51 squash-merged (CI green); tag and release v0.1.10 (`madnesswright-0.1.10.zip`, 121,552 bytes); wiki to 0.1.10 (Versions row, Getting-Started hash, footer, Home). Workshop Change Notes tab read in the browser before the upload: it showed 0.11.0. Upload with the owner's OK: `result OK`, GetPublishedFileDetails title "Director", visibility 0, 57 subscribers, updated 2026-10-29 22:20:51 UTC; change notes in `examples/SwapUnaware/change-notes-0.12.0.txt` say not played.
- Not played (HANDOFF Owed 19). Skipped by standing ruling: evergreen refreshes of other units.

## 2026-10-28 · Director 0.11.0 first play: logs clean

- Report: the requester said infinite ammo did not work (on their own machine); they could not reproduce it on the owner's PC. Likely an old Director there (the owner's previous Player.log was 0.10.0, which had no infinite ammo); not confirmed.
- Player.log 2026-10-28 18:44 (Director 0.11.0, one Playground session): 0 exceptions, mods.log clean. Backslash used 10 times across 6 bodies, including after F5 swaps; one off; 4 "no character under the mouse" misses. Heavy use of F2 freeze + F6 unaware on one character in turns (about 30 lines).
- Previous log (0.10.0, 2026-10-25) had scene-spawn NullReferenceExceptions (AAHW2_Engineer, Arena_Troop_B): the `CallSquadmates` issue already logged below.
- Improvement ideas, not built: backslash with no one under the mouse falls back to the player's own body; one key for freeze + unaware together.

## 2026-10-28 · Director 0.11.0: infinite ammo and Spawn a weapon

- Request (the requester, via the owner): infinite ammo on a character picked with the mouse and a key, toggled; spawn any weapon in the game, including ones the Playground, loadouts and shops do not offer, from a category list then a weapon list, uncategorised ones in their own group.
- Decompile (scratchpad only; build still 25615196): two per-character flags exist. `Combatant_Base.infAmmo` (InfiniteAmmo trait) makes `SpareMag_Have`/`SpareMag_Spend` always true, so reloads play and spare magazines and thrown restocks never run out; `SetTraitBools` recomputes it (`ApplyAllStats`: spawn, `DeathEvents`, arena shop; boss patterns). `Target_Object.buff_InfAmmo` (powerup, default 1) multiplies rounds spent and melee wear. Catalog: `StatCard_Base.ReturnAllWeapons()`; the Playground (`InteractiveMode`) filters `ItemEnabled`, store tier, not Arcane, drops Unarmed and None, and its exact-type `ReturnAllCardsByType<StatCard_Weapon>` misses mods' guns and throwables. Spawn: `Catalog_Weapons.GenerateWeapon`, `Char_DataAssigner.GiveItemToCharacter`/`GiveThrownToCharacter`, `Item_Held.CreateHeld`. Crash risks from code: `myRef` null, `StatCard_WeaponModded` without `LinkedWeapon`; `CollectHeld` silently refuses when the held item cannot be dropped. Keys: backslash is never read. Note: [notes/2026-10-28-ammo-and-weapons-research.md](notes/2026-10-28-ammo-and-weapons-research.md).
- Rulings (README): the trait flag, not the powerup (reloads stay, melee wears); key backslash; any living character, players included; green/white flash; stays with the body through F5; death ends it; scenes save it (version 7; older files load with it off; the keep box leaves players' alone). Spawn a weapon: INSERT entry, game's categories in enum order, Other last, sorted by name with the card name added when two share a name, everything listed, Playground-hidden marked *, mods marked (mod), broken cards left out and logged; into the hands by default (floor when ticked or when the hands hold an undroppable item); window holds its own pause; ESC back then close.
- Built as `AmmoFeature`, `WeaponSpawnFeature` and plain `WeaponCatalog` (grouping). Harness (`tests/DirectorFreezeChecks`) gained infinite ammo checks (on, off, trait kept, flag put back, F5 and F8, death, load clear with and without the keep box) and list checks (order, Other, sort, duplicates, labels); both written to fail first. Pack 0 warnings, check-mod clean, deployed, DLL byte-identical with `0.11.0` as UTF-16; check_skills, test_mpn, test_decomp pass; budget yellow (tracked 1,188 KB).
- CI: ubuntu-latest became Ubuntu 26.04, which has no Python 3.9; the structure matrix now runs on ubuntu-24.04. PR #48 squash-merged.
- Published with the owner's OK: Workshop item 3822329165 at 2026-10-28 22:49 UTC, `result OK`, visibility 0, 54 subscribers, description has Spawn a weapon and the madnesswright line; change notes say not played (the Change Notes tab text was not read back). Plugin 0.1.9 (PR #49), tag and release v0.1.9 (`madnesswright-0.1.9.zip`, 121,552 bytes). Wiki: version mentions to 0.1.9 (outputs still from 0.1.7's unchanged scripts), Versions rows for 0.1.8 and 0.1.9, Director paragraph to 0.11.0.
- madnesswright: game-recipes lines (infinite ammo, weapon catalog and spawning, two-step IMGUI list); modtools L-008 (look for the game's own flag and read a model list's filter and lookup before patching or copying). No TESTS.md case: the harness covers the regressions.
- Not played (Owed 18). Skipped by standing ruling: evergreen refreshes (everlast-resume reported due 2026-10-28).

## 2026-10-28 · Director 0.10.2: a body a player drives is never frozen

- Report (the requester, via the owner): since 0.10.1, after F5 and F2 the body they controlled did not always unfreeze. Mark's ruling: a body any player controls is never frozen; leaving a body that was frozen when taken freezes it again.
- Decompile (scratchpad only, same build 25615196): `Controller_Base.LockSources` is a `HashSet<UnityEngine.Object>`; `LockController(src)` adds, `ReleaseController(src)` removes, `ReleaseController()` clears, `AmLocked()` is count > 0; `SetConnection` to a new body runs `NewConnection`, which clears every source. `Controller_Player.LockController` also deselects squadmates. `Combatant_Base.AssignController(c, true)` gives the incoming controller's old body this body's controller (both swap paths end there; `SwapControllersWithSquadmate(Combatant_Base)` calls it). So the prompt's guess of a counted lock was wrong; the stub's set was right.
- Cause: 0.10.1 by design moved a frozen body's lock onto the player's controller when F5 took it over, so F5 onto a frozen body, or F5, F2 (which freezes the home body), F8 home, left the player locked until the next F2. Also found: when the game swaps two frozen bodies' controllers itself, the per-body rebind released the lock the other body had just taken.
- Harness: stubs now clear locks on a new connection and have `PlayerControllers`; sequences run on the cross-squad and same-squad paths: 0.10.1's order, the requester's F5-F2-F8-F2, chained F5 across frozen bodies, F2 while inside one, a game swap of two frozen NPCs, and a stray player lock. Failed on 0.10.1 at "F5 onto a frozen body: the player's controller is locked"; passes on 0.10.2; a one-pass rebind mutant fails "game swap".
- 0.10.2 (`FreezeFeature`): a player's body stays listed but holds no lock and is not pinned (`held` false) and is held again when the player leaves; the tick lets go of every stale lock before taking new ones; `ReleaseAll` also releases Director's source on every `Controller_Player.PlayerControllers` entry. Built (0 warnings), `--strict check-mod` clean, deployed byte-identical, DLL holds `0.10.2` as UTF-16. Not played: the change notes say so.
- Plugin: modtools L-007 (read the decompiled member before stubbing; test the user's key order), T-20261028-1, a lock note in `references/game-recipes.md`.

## 2026-10-27 · Director 0.10.1 on the Workshop

- The owner asked to publish the freeze fix before pushing the repository changes. `pack.ps1 -Version 0.10.1` built the mod against the installed game assemblies; `mpn.py --strict check-mod` reported no errors or warnings. The staged DLL matched the built DLL byte for byte and contained `0.10.1`. The F10 and F5 takeover/switch-away C# regressions, 31 Python tests, skills check and budget check passed.
- Workshop item 3822329165 updated in place: `result OK`, no legal agreement needed. The public listing (visibility 0) reported the new F2 description and a 2026-10-27 22:22:19 UTC update; its public Change Notes tab contained `Director 0.10.1`. The change-notes text is in [the mod folder](../examples/SwapUnaware/change-notes-0.10.1.txt).
- Not played in the game yet. Try F2, F10 (all three states), F5 onto a frozen character, F8 away and F2 to release it. The older F5-then-scene-load issue and arena co-op playtest are still owed.

## 2026-10-25 · Director 0.10.0 on the Workshop

- The owner, tired of testing, asked to release what we have. Workshop item 3822329165 updated to 0.10.0 (the deployed, byte-identical DLL, holds `0.10.0` as UTF-16): `result OK`, GetPublishedFileDetails visibility 0, updated 21:55, 47 subscribers.
- `workshop-description.txt` gained arena co-op (0.9.0) and keep-box (0.10.0) paragraphs. The change notes (`dist/change-notes.txt`, not tracked) cover 0.9.0 and 0.10.0, list the F5-then-load empty scene as a known issue with its workaround (untick the box or press F8 first), and say arena co-op has not had a full play test.
- Still owed: the 0.10.1 fixes (Owed 17) and the arena co-op play (Owed 15), now on a public build.

## 2026-10-25 · Director 0.10.0 first play

- The requester played 0.10.0 in the Playground (InteractiveChamber only) and answered an askwright test page. Player.log agrees the box works: ticked loads end "you kept your character and weapons", unticked ones "your weapons reset". The first load after F5 left out the played body as designed (8 of 9).
- Failed: the next load of that scene spawned 0 of 9; every spawn threw `NullReferenceException` in `Orders_Manager.CallSquadmates` (via `Squad_Main.AddMember`, `Target_Object.ChangeFaction`, `Room_Main.UpdateFactionPresent`), and so did `ClearRoom`'s `RemoveFromGame`, aborting the restore. Lead: a squad still lists a member whose combatant is gone.
- Asked for: the Playground load should move the player back to the saved spot (story and arena loads already do). Every Playground load also reports 4 or 5 of 5 characters off their spot 2 s later; unexplained.

## 2026-10-25 · Director 0.10.0: keep my character and weapons on load

- The requester asked for an option to keep the character and weapon they have when they load a scene, maybe a checkbox on the load page. No play since 2026-10-24 22:15 (Player.log), so the design came from the code and a fresh decompile (same build 25615196).
- What a load did to the player before: the body you control was never swapped, but the Playground load replaced your weapons with the saved ones, a story or arena load reset your ammo, a scene saved on another Playground map loads that map (which may hand you its own character), and a saved record of the body you play now (taken with F5 after saving) was spawned again as a double.
- 0.10.0: a box on the CAPS LOCK list, remembered as `PlaygroundDirector/load-keeps-mine.txt`, off by default. Ticked: `SceneData.keepMine` skips `ResetLoadout`/`RefillAmmo` for you and for pad players; a cross-map Playground load snapshots your card and `Loadout` first and afterwards calls the game's `InteractiveMode.PlayCharacter_Done` (its `currentInteractive` and `selectedPlayCharacter` are non-public, set through `AccessTools`) when the card differs, then gives the weapons back. Always: `PutPlayers` leaves out the record whose `myUniqueID` is your body's.
- Clean build (0 warnings), deployed, DLL byte-identical to the build and holds `0.10.0` and `load-keeps-mine` as UTF-16. Not launched or played (Owed 17).

## 2026-10-25 Â· Workshop change notes

- The owner noticed the item's Change Notes tab said only "Update" for the last three updates: `examples/WorkshopUpload/Program.cs` hard-coded `WithChangeLog("Update")`. It now takes a change-notes file as the seventh argument and exits 2 before `SteamClient.Init` when an update has none or it is empty (checked: `wsup.exe a b c d e 123` prints the error, exit 2). Rule in AGENTS.md, madnesswright Step 6 (C-20261025-1); backfilling the three old entries is Owed 16.
- Backfill done the same day on the owner's "go": the tab held six "Update" entries, not three (Steam shows times in the viewer's zone). Each entry has its own page, `steamcommunity.com/sharedfiles/editchangelogentry/<item>/<entry>/`, with one textarea and a Save link; filled in 0.4.0, 0.6.0 (which also carries the never-published 0.5.x scene work), 0.6.1, 0.7.4 (which carries 0.7.0 to 0.7.3), 0.7.5 and 0.8.0 from the root CHANGELOG, and checked on the Change Notes tab.

## 2026-10-24 · session 16: arena co-op researched, owner chose the pad origin picker, Director 0.9.0 built

- No play since session 15 (Player.log 22:15). Same build 25615196; fresh decompile in the scratchpad. Findings in [notes/2026-10-24-arena-coop-research.md](notes/2026-10-24-arena-coop-research.md): an arena career is built around one character with one `StatCard_Origin`, picked at career creation or a new imprint; hired squadmates come only after the pilot rescue (`HQ_Stats.Value_Hireling_SquadMax` 0 before); no half-built co-op path; rewards go to `chars_Core`; a character with no career never ends the wave; no revive in arenas. This PC has no arena career.
- Plan with three routes: [plans/2026-10-24-arena-coop.md](plans/2026-10-24-arena-coop.md). One question to the owner; they chose (b), the pad origin picker.
- 0.9.0 (`ArenaGuestFeature`): a pad waiting in `PlayersWaitingForAdd` in an arena stage (not the HQ) gets an IMGUI panel driven through `Joystick_Manager.Check_DpadPressed` and `Check_ButtonPressed`; the pick builds a guest (`Char_Data.CreateCharacter` on the "Arena" card, `ApplyOriginAndResetFromStats`, `Career_Data.CheatCharacterLevel` to player one's level), adds it to the player squad, spawns it beside player one with a bot and hands it the pad's controller with `AssignController`, plus the origin's first `WarchestStarterWeapons`. Not in the career. A dead guest returns when the wave is over; a guest whose player left is poofed; guests leave the squad on every scene load. Picks in `PlaygroundDirector/arena-guests.txt`.
- Clean build (0 warnings), deployed, DLL checked (UTF-16 strings `0.9.0`, `arena guest`). Not launched or played. Checks: check_skills, test_mpn, test_decomp pass; budget yellow as before.

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
## [2026-11-11] Nexus Prominence: room 3 ammo box on the floor

- Continue-prompt session; owner said keep going, will test later. `Stations` now stands a station on the floor (80% back, 35% across) when no back-wall patch is clear; room 3's ammo box was the only one skipped. Rebuilt headless (build-g, shots-g, sync-g logs in madnesswright-lab/nexus), 40/40 corners OK, check-mod clean, deployed. **Not played, not uploaded** (Owed 24). Untried fun ideas still: reinforcement waves, vent/rappel spawns, Spikes.

## [2026-10-23] index | rebuilt (13 entries)
## [2026-10-24] index | rebuilt (15 entries)
## [2026-10-24] index | rebuilt (15 entries)
## [2026-10-24] index | rebuilt (17 entries)
## [2026-10-25] index | rebuilt (17 entries)
## [2026-10-28] index | rebuilt (18 entries)
## [2026-10-29] index | rebuilt (18 entries)
## [2026-10-30] index | rebuilt (18 entries)
## [2026-11-11] Nexus Prominence: room table on the Workshop, fun pass

- The owner tested the camera, dressing and clean-up build and said publish. wsup.exe updated item 3837705778 with change-notes-2026-11-11e.txt and madnesswright-lab/nexus/workshop-preview-2026-11-11.jpg: result OK. Description gained a room table (contents, enemies, difficulty Easy to Very hard) in Steam BBCode `[table]`.
- Fun pass: generator `Stations` instantiates Assets/Library/Prefabs/Proxies/PX_Interact.prefab (Type Ammo, AmmoGrenade, MedCabinet) on free back wall (reserves a wallBusy rect so signs avoid them) and Markers/Item_Spawn.prefab with a Nexus MadCard per room (R_NN57, M_SwordNexus, R_NM46, Shield_RiotLight, R_Hrifle_NFAL, R_NZucker, R_Nc762, R_NS-V.10). First try searched u 0.06-0.6 and dropped two stations; widened to 0.95, room 3 still has none.
- Built, NexusShots -corners 40/40 OK, SyncAndBuild (.sobj 1.95 MB), check-mod clean (no Author), deploy --force. Not played, not uploaded.

## [2026-11-11] Nexus Prominence: released, then rooms resized and filled

- The owner tested the rooms 1-2 build: "release it". wsup.exe updated item 3837705778 with change-notes-2026-11-11b.txt (result OK). Gotcha: wsup.exe needs absolute Windows paths for every file argument; a relative path joined to $PWD threw FileNotFound at Program.cs line 16.
- Asked: can rooms only be square? They were because every room is a clone of the one sample box. New `Stretch(room, width, depth)`: room meshes and colliders scale about the left and back walls, everything else (doors, cover, markers, lights, the four camera borders) moves, the camera keeps its distance. Two bugs on the way: collider bounds were read before `Physics.SyncTransforms`, and the imported meshes are rotated (-90 on X), so each local axis is now scaled by the world axis it lines up with. Widths: 47, 41, 31, 50, 38, 43, 63, 27, 36, 47 units.
- Camera check (decompile, Cam_Main): the game clamps the camera between the room's borderX positions, which come from Border_Left and Border_Right, so a wide room pans instead of cropping.
- Look: side-wall diagonal red stripes, banners (an original chevron, no logo), corner posts, floor grates, papers, a red floor line, back doorways with hazard headers; rooms 3-10 gained cabinets, vending machines, a fridge, a chalk slate, a sky strip over the monitor wall. Cover spreads through the floor by (across, depth) spots and squads grew (3 to 7, riot guards and soldats mixed in), staggered in depth.
- Rebuilt, SyncAndBuild (.sobj 1.71 MB), check-mod clean (no Author), deployed. **Not played, not uploaded.** Render: examples/NexusProminence/renders/rooms-2026-11-11c.jpg.

## [2026-11-11] Nexus Prominence: Workshop updated, rooms 1-2 closer to the video

- The owner tested rooms 1 and 2 and said to publish (the old version "will make us look bad"). wsup.exe updated item 3837705778 with the rebuilt map and workshop/change-notes-2026-11-11.txt: result OK.
- Asked for rooms closer to the video. Pulled full-size frames (0:23-0:58 street, 1:30-1:56 wanted hall) into Ai/madnesswright-lab/nexus/ref12 and compared with NexusShots renders. Room 1 now: stained dark floor, window over three quarters of the back wall (taller towers, billboards, frame, glowing sill, counter rail, hung sign), dark pillar with a bin and cups, medkit cabinet, recessed door panel with a red board, striped skirting, wall seams. Room 2: red light and ambient, three large framed wanted-style posters on rods (no faces copied), a glass display case with shelves, two dark doorways, red conduits, two floor hatches, band stripes.
- Limit: the game camera (CameraPoint) looks down on a deep floor, so a frame can never be identical to the video's flat side view; the back wall is where the match happens.
- Rebuilt, SyncAndBuild, check-mod clean, deployed locally. **Not played; this version is not on the Workshop yet** (the Workshop has the earlier 2026-11-11 rebuild). Render: examples/NexusProminence/renders/rooms-1-2-2026-11-11b.jpg.

## [2026-11-11] Nexus Prominence rebuilt to look like the video

- Report: the requester said the levels "looked nothing like the video", and the owner saw a light red fog in the default room. Cause: every room was the bare sample box, and in a headless render it showed as one dark red box (proxies only appear in game). The fog was the sample's FogDensity 0.1 plus our red FogColor and ambient.
- Reference: re-pulled v0suvMY3GgM (`uvx yt-dlp -S res:720`, saved as video.mp4.webm) into Ai/madnesswright-lab/nexus (not in the repo: the video's art is not ours), with contact sheets at 1 frame per 4 s.
- Generator: a shared shell per room (red band at 1.05-1.6 above the floor, red skirting, red floor seams on three walls, back-wall pilasters, a pipe rail, a black opening with dark jambs and a red-and-dark hazard bar over every side door), plus a set for each room built from the frames as plain primitives with no colliders (no logos, posters or characters copied): street window onto a block-tower skyline, framed boards with a floor hatch and a vending machine, a counter under a glowing window, strapped crates with a floor hazard line, a ledge with a ladder and an open-frame crate, a roll-up shutter over a console, four ceiling lamps throwing additive red cones, lockers and a bench, a serving counter under menu panels, and a monitor wall with server racks. Fog off (density 0, GlobalFog disabled), ambient 0.30 gray, white light.
- Bugs fixed: (1) rooms 2-10 were cloned from room 1 after it was dressed, so every clone carried room 1's enemies and set (now: clone everything first, then dress); (2) the overwrite dialog cancels in batch mode (skipped there).
- New `Editor/NexusShots.cs`: renders each room from its CameraPoint to PNG with only that room's lights on (all ten directional lights otherwise wash everything out). Grid: Ai/madnesswright-lab/nexus/rooms-2026-11-11.jpg (out of the repo for size).
- Rebuilt headless, SyncAndBuild, .sobj 1.39 MB, check-mod clean (warning: no Author), deployed with --force. **Not played. Workshop item 3837705778 not yet updated** (waits on the owner's OK).

## [2026-11-10] Nexus Prominence map template

- Ask: template of the map in YouTube v0suvMY3GgM (collab, 14:22). Pulled it with `uvx yt-dlp -S res:480` (plain `-f best[height<=480]` found no format), contact sheets with ffmpeg `fps=1/9,tile=4x6`.
- No in-game editor, so the route is SceneTools. Wrote `examples/NexusProminence/Editor/NexusProminenceBuilder.cs`: copies SamplePlayground_PG, clones its one room (5 doors) ten times 300 units apart, links rightmost door to next room's leftmost, keeps Player_Start and PX_Playground in room 1, repaints with Standard materials, adds Char_Start markers (Nexus1-6, Scientist; CharacterTypes from SceneEditorHelpers.cs).
- Unverified: never compiled (no Unity 2019.2.6f1).

## [2026-11-11] docs | Works with section
README gained a Works with section (polywright, musicwright, fetchwright, ludowright); the wiki gained a Works-with page linked from the sidebar and Home. Each entry describes a handoff found in the skill files on one side or the other.

## 2026-11-11 (budget)

- Continue prompt run: no owner play since the Stations rebuild (mods.log 11:11, Player.log 11:17), so no generator work. Shrank .claude-plugin/icon.png 512 px RGB 283 KB to 256 px FASTOCTREE palette 18 KB (Pillow); budget tracked KB 1,575 to 1,317. PR #78 merged, v0.1.15 released.
- The owner played the Stations build: "looking good release it". wsup.exe updated item 3837705778 with change-notes-2026-11-11f.txt and workshop-preview-2026-11-11.jpg: result OK; GetPublishedFileDetails shows visibility 0, updated 13:12, 21 subscribers.

## 2026-11-11 (juice released)

- Owner: "Play test went well release the latest version". mpn.py log clean. wsup.exe updated item 3837705778 (change-notes-2026-11-11g.txt, workshop-preview-2026-11-11.jpg): result OK; GetPublishedFileDetails: visibility 0, updated 14:02, 24 subscribers, description has the breakables.
- Scenes skill: L-20261111-3 and a rules line (PR #82, v0.1.16). game-juice juice-apply L-002 (PR #12, v1.3.5). Both plugins updated locally.

## 2026-11-11 (spike fix)

- Owner: "Play test went well release the latest version". mods.log clean; Player.log 3x NRE Toggleable_Automator.SetAutomatorState (Init from Toggleable_Controller.Awake). wsup.exe updated 3837705778 with change-notes h (spike timer listed as a known issue): result OK, live 14:32, 26 subscribers.
- Fresh decompile (scratchpad, game Assembly-CSharp): Converter_Proxy.ReplaceProxy instantiates a library prefab for stock togglables and copies no add-ons; ConvertCustomObj adds Toggleable_Main onto a CO_ Togglable; UpObjs entries are swapped for created objects. Generator `Spikes` now uses a CO_ driver. Built, 40/40 corners, deployed; not played. Scenes L-20261111-4, PR #85, v0.1.17.

## 2026-11-11 room 1 rework

- The owner asked to make room 1 smaller and rework its sides and back after the film frame at 0:19. New `Street` set (one back window, left counter, right side door) and RoomSpec 1.0 x 0.8 with 2 agents. Decal() quads only face the camera, so side-wall signs cannot use it: "no loitering" moved to the window head. build-m, shots-m (40/40), sync-m, check-mod, deploy OK. Workshop upload blocked by the auto-mode classifier; see HANDOFF.

- 2026-11-11 16:50: merged PR #87 (room 1 rework), tagged and released v0.1.17, updated the installed plugin. Upload still waits on the owner.
- 2026-11-11 balcony pass: owner wanted no roof or foreground in view (open-air balcony). DepthScale 0.7 on all rooms, camera FOV x0.8, DepthFollow 0.25. build-n, shots-n 40/40 corners, sync-n, check-mod, deploy OK. Not played.

## 2026-11-11 continue check
Balcony build unplayed (mods.log 16:47 < deploy 16:52). No work possible; waits on owner.
Recheck: played 16:53-16:55, logs clean apart from a stock NRE on exit to menu (Controller_Player.OnDisable). Waits on owner verdict + upload OK.
- 2026-11-11 tighter pass: owner said still too wide/zoomed out. WidthScale 0.8, DepthScale 0.6, Zoom 0.65. build-o, shots-o 40/40, sync-o, check-mod, deploy OK 17:00. Not played.
- 2026-11-11 narrow+deep skyline pass (build-p): WidthScale 0.6, Zoom 0.55, EdgeLead 6, camera z follows half the removed depth, room 1 walls mesh hidden and rebuilt, 3-layer city behind glass. Gotcha: sample walls are one mesh (back+sides). Far layers at 40/60 rendered black; 24/32 work. 40/40 corners, deployed. Not played.
- 2026-11-11 build-q: room 1 blood splats removed (Gags `blood = index != 0`); film contact sheets show blood in every later room. 40/40, deployed 17:09. Not played.
- 2026-11-11 wrap-up: L-20261111-5 `sample-walls-one-mesh` added to madnesswright-scenes LEARNINGS (uncommitted, ships with the next PR); lab script madnesswright-lab/nexus/rebuild.sh <letter> runs a whole pass.

## 2026-11-11 revert to build-m look

State on 2026-11-11 (revert): the owner said builds n-q "fucked it up", the Playground room too. Generator reverted to the committed room 1 build (PR #87, build-m): no WidthScale/DepthScale/Zoom, DepthFollow 0.6, EdgeLead 10, no OpenBackWall, old flat city in the window. Kept only: no blood in room 1. build-r, 40/40 corners OK, check-mod clean, deployed 17:13. Learning L-20261111-5 retired as harmful. **Not played, not uploaded.** Next: owner plays build-r; narrowing, if still wanted, goes in small steps (e.g. WidthScale 0.9 only) with a play between each.

## 2026-11-11 release build-r, narrower, left wall

State on 2026-11-11 (left wall + narrower): on the owner's go-ahead, build-r was uploaded to Workshop item 3837705778 (wsup.exe, change-notes-2026-11-11j.txt plus a no-blood line, lab preview j; result OK). Then: every room's width x0.85 (`WidthScale`, width only; depth, zoom and camera follow untouched), and room 1's left wall matches the film at 0:19: plain charcoal facing (`FacingL`), vertical panel seams, a dark rail, red skirting; the counter, red band, diagonal stripe, banner and the three doors' hazard bars on that wall are gone (Shell pieces are named `NX_*`, so the strip matches that prefix). build-v, 40/40 corners OK, check-mod clean, deployed 17:21. **Not played, not uploaded.** Next: owner plays build-v; upload only on their OK.

- 2026-11-11: continue session: merged polywright #55 (budget fix, v0.6.4 released), madnesswright #61; build-zd unplayed, waiting on owner.
- 2026-11-11: added skills/madnesswright-scenes/harvest.json (5 community targets for level design: GameBanana, Steam discussions + Master Doc, Steam guides, r/madnesscombat, YouTube search; Discord note only). No madnesswright everscout beat exists, so harvest.json is the only home.
