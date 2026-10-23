# Handoff

State on 2026-10-22, session 11c: Director 0.7.4 played and passed (scenes reset characters, weapons, bodies and blood in story levels) and published to Workshop item 3822329165 (public, 35 subscribers). Open: the 2 s drift check flags most characters after a load (Combat ones run off; the owner saw no problem). Plugin v0.1.8 tag still waits on the owner. Session 11 read the never-exercised paths in the decompile (INSERT works between arena waves, so scenes can be saved there too; guard lacks the spawner's `noRagdollInCombat`; details in [log.md](log.md)). Session 10: Director 0.7.3 (bodies, props and drift on scene load) deployed, not played. Session 9: Director 0.7.0 (scene loads reset weapons and items) built and deployed, not yet played or on the Workshop. Before that, session 8: Playground Director 0.6.0 (Director menu on INSERT, clear corpses) merged, released as plugin v0.1.7 on the owner's go-ahead and published to Workshop item 3822329165 before its playtest (the owner chose that); wiki at 0.1.7; eval fixes (Owed 3 to 5). Details: [log.md](log.md).

## Session 10

- Report on 0.7.2: dead enemies' bodies stayed after a load, a laborer's crate was never given back, one enemy ended up off its spot. Director 0.7.3 (modtools T-20261022-7, [log.md](log.md)): scene files version 4 remember the bodies lying at save time and the load clears newer ones; unregistered prop cards found among loaded cards; post-save walk orders dropped; per-character load lines plus a 2 s drift check in Player.log. Needs a play (Owed 12).
- Session 9: scene loads reset weapons and items (0.7.0 to 0.7.2; [log.md](log.md)).

- Session 8 menu design: each `Feature` adds `MenuEntry` lines in `Register()`; a keyed line presses its key for one frame (`Feature.Pressed`), character-aimed lines ask for a click first. Clear corpses uses the dev console's route (`HaveRunKillMe`, `Decay_Manager.DestroyMe_Character`) and keeps squad, scripted, boss and essential bodies. Only a load smoke test so far: the playtest checklist is in the owner's private prompt.
- WorkshopUpload: the csproj now copies `steam_api64.dll` beside the exe, and the uploader needs absolute paths (relative ones end in `result FileNotFound` after 100%).

## Earlier sessions

- Session 7, 0.5.0's first story playtest: loads always said "room , closed" because `JsonUtility` dropped the scene's room and characters (the mod's own nested classes; 0.4.0 too), and one save crashed in `AmDead()` on an unset corpus track. 0.5.1 fixes both (modtools L-005, T-20261022-1; [log.md](log.md)). Second play passed (T-20261022-2).
- Session 6, F11 scenes in story levels and arenas. Decision and limits: [decisions/2026-10-21-scenes-in-every-level.md](decisions/2026-10-21-scenes-in-every-level.md); hooks: [notes/2026-10-21-scenes-in-levels-research.md](notes/2026-10-21-scenes-in-levels-research.md).

## Session 5

- Request: eight new keys for the swap mod's player (F1 clear, F2 freeze, F3 patrols, F4 idle animations, F9 guard, F10 alert cycle, F11 saved scenes). Decision: grow the existing mod, renamed Playground Director, one component per feature ([decisions/2026-10-21-update-swap-mod-not-new.md](decisions/2026-10-21-update-swap-mod-not-new.md)). Plan with rulings and the player's checklist: [plans/2026-10-21-playground-director.md](plans/2026-10-21-playground-director.md). Game hooks: [notes/2026-10-21-director-mod-research.md](notes/2026-10-21-director-mod-research.md).
- No key moved: none of the requested keys is read in normal play (CapsLock is autowalk; read only while paused).
- Playtested on the owner's PC (modtools T-20261021-1: F1 to F4, F10 and F5 to F8 seen in Player.log, no exception; F9 guard and F11 scenes not yet exercised, confirm next report). Published as an update of public item 3822329165 (title "Playground Director (was Swap And Unaware)", new preview), still public, 28 subscribers. Wiki worked example updated.
- Plugin v0.1.5 tagged and released (the owner's go-ahead; 0.1.3 and 0.1.4 are folded into it): `scripts/decomp.py` (decompile, keys, find, callers; 5 tests in CI), `references/game-recipes.md`, L-003, L-004.
- Likely cause of the 0.3.0 "teammate turned on me" report: Playground spawns are `Factions.None` squads, hostile to everyone outside their squad. Budget: still yellow (mpn.py 9,500 tokens; installed skills folder 205 KB, green up to 200).
- Sessions 3 and 4: beta launch passed (build 25615196: [notes/2026-10-18-beta-launch.md](notes/2026-10-18-beta-launch.md)); swap mod 0.1.0 to 0.3.0 built and fixed (L-002, T-20261018-2; details in [log.md](log.md)). Source and build in [examples/SwapUnaware](../examples/SwapUnaware/README.md); public Workshop item 3822329165 uploaded and updated with [examples/WorkshopUpload](../examples/WorkshopUpload/README.md) (needs the uploader zip's Facepunch DLLs in `lib/`; auto mode asks the owner once). Loader diff release vs beta impossible while on the beta.

## Done

- Public: https://github.com/m4bwav/madnesswright (topics set, private vulnerability reporting on, CI green on Windows, macOS and Linux with Python 3.9 and 3.14). Releases v0.1.1 and v0.1.2 (https://github.com/m4bwav/madnesswright/releases/tag/v0.1.2) from `.github/workflows/release.yml`. 0.1.0 was never tagged.
- Wiki live at https://github.com/m4bwav/madnesswright/wiki (10 pages, names 0.1.7 since 2026-10-22): how it was made and how to update it in [notes/2026-10-17-github-wiki.md](notes/2026-10-17-github-wiki.md).
- Real-game check on the owner's install (release v1.11.a, build 25615280) and a decompile of its mod loader: [notes/2026-10-17-real-game-loader.md](notes/2026-10-17-real-game-loader.md). Facts folded into the skills (hub R-20261017-9, C-20261017-3; modtools R-20261017-6, C-20261017-3 to -5). The big ones: the log is `Mods/mods.log` (the doc says mods.txt), `Mods/enabled.txt`, info.json allowed, `.json` cards only as a container, `IModEntrypoint` in the global namespace, mods target net48.
- Installed locally as `madnesswright@mark-local` (entry in the Ai folder's `.claude-plugin/marketplace.json`).
- Tests: 26 unit tests; value cases 3 of 3 runs on all six (one fail on run 2 of modtools outcome-1 fixed the net48 rule); baselines for all six; triggers and decoys 36/36 headless after a routing fix (first pass 30/36: the scenes skill never fired). Each TESTS.md has T-20261017-3 and -4.

## Owed

2. **Listings on hold** by the owner's ruling (2026-10-17): no Claude directory submission, and never awesome-copilot. Do not submit.
3. Done 2026-10-22: `Arena_PG` lives in `tests/fixtures/arena-project` (no reference scene). Skill pass, but the baseline still found both faults from the prefab Library (scenes T-20261022-1): the case separates on cost, not correctness.
4. Done 2026-10-22: outcome-1 expects one PatchAll (the loader's). Skill and baseline both passed (modtools T-20261022-3); the baseline needed a second build (net472 first).
5. Partly done 2026-10-22 (scenes T-20261022-2): `eval-export --out evals/<skill>` works (the default `skills/<x>/evals/cases` is refused as `--eval-dir`); triggers +1, decoys quiet; value cases cannot run on native Windows (no sandbox for Bash; outcome exported read-only). `worth --record`: UNPROVEN. Next: `worth --ab` or WSL2.
6. Size budget is yellow (2026-10-22): `mpn.py` about 9,500 tokens (green up to 9,000; comments and docstrings are only 2.6K characters, so a trim means refactoring tested code), the hub description 1,000 characters (trim needs the 36-run trigger eval), installed skills folder 211 KB.
7. Headless SceneTools build route is untested (needs Unity 2019.2.6f1).
8. Evergreen refresh: skipped this session by ruling. The units are due 2026-11-16; the volatile releasebeta claim is due for recheck 2026-10-31.
10. Director 0.6.1 playtest (renamed from Playground Director 2026-10-22; on the Workshop): Director menu, clear corpses (story room, arena between waves, Playground), arena mid-wave scenes, Playground scenes (also from another map), story room-closed path, die-Retry-load, F9 guard (never logged yet). Already on the Workshop, so fix fast if it fails.
11. Done 2026-10-22: Director menu and clear corpses (0.6.0, plugin v0.1.7).
12. Director 0.7.3 playtest (also: kill enemies, load, no bodies left and every enemy on its spot with its weapon; read the per-character lines) (weapons reset on scene load): in the Playground and a story level, save a scene, drop or throw weapons, kill someone, empty a magazine, then load: everyone holds what they held with the same ammo, floor weapons are back where they lay and new ones are gone; Player.log's load line counts them. Then a Workshop update (owner's OK).

Next step: the owner keeps session prompts in private notes (they name local paths). Without one: Owed 12 and 10 (read Player.log and the newest scene file after the next play), then Owed 5 via `evergreen.py worth --ab`.
