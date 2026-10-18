# Handoff

State on 2026-10-18, session 4 (swap mod 0.3.0, example in the repo, on the Workshop; 0.1.4 merged, not tagged).

## Session 4

- Swap mod 0.3.0 fixes the mid-fight take-over failure (duplicate AI target claimants; see log and L-002). Deployed; the owner's playtest is still owed (F5 mid-fight after F6, arena take-over then F8, a squadless spawn dying, F5 while dead), then T- entries in modtools TESTS.md (T-20261018-2 is cited by L-002 and C-20261018-2 but not yet written).
- Source now lives in [examples/SwapUnaware](../examples/SwapUnaware/README.md) (PR #2) and builds from there with `pwsh ./pack.ps1 -Version X`.
- Public Steam Workshop item 3822329165 (https://steamcommunity.com/sharedfiles/filedetails/?id=3822329165), uploaded 2026-10-18 with a small Facepunch.Steamworks program through the owner's logged-in Steam (no credentials handled). Visibility public by the owner's ruling. Still owed: Subscribe on the second computer and check `mods.log` lists it as `[WORKSHOP]` (do not subscribe on the dev PC: the local copy would load twice).
- Loader diff (release vs beta): not possible now, the install is on the beta and the release DLL is gone; the release notes from session 2 stand alone.
- Still owed: 3, 4, 5 (eval fixes) and the size budget. Tagging 0.1.3/0.1.4 waits for the owner.

## Session 3

- Beta launch done: build 25615196, Beta v2.20.a, GreenBloodHank loaded and the owner saw green blood. Notes: [notes/2026-10-18-beta-launch.md](notes/2026-10-18-beta-launch.md). Owed 1 is closed except the release-vs-beta loader diff (decompile both, compare Mod_Manager, ModItem, Converter_JSON, Game_Manager).
- Swap-and-unaware mod (Owed 9): built, deployed, played. 0.1.0 crashed in the game's KillMe when a taken-over squadless Playground spawn died, and could not swap back out of a locked arena. 0.2.0 (one Prefix, screen-space pick, F8 home) is deployed and waits for the owner's playtest. Source lives outside this repo.
- 0.1.3 merged (PR #1): lessons in modtools L-001 and harmony.md, `mpn.py log` stack frames and card lines. Not tagged: tagging is the owner's call.
- Still owed from the list below: 3, 4, 5 (eval fixes) and the size budget (mpn.py now about 9,500 tokens).

## Done

- Public: https://github.com/m4bwav/madnesswright (topics set, private vulnerability reporting on, CI green on Windows, macOS and Linux with Python 3.9 and 3.14). Releases v0.1.1 and v0.1.2 (https://github.com/m4bwav/madnesswright/releases/tag/v0.1.2) from `.github/workflows/release.yml`. 0.1.0 was never tagged.
- Wiki live at https://github.com/m4bwav/madnesswright/wiki (10 pages, names 0.1.2): how it was made and how to update it in [notes/2026-10-17-github-wiki.md](notes/2026-10-17-github-wiki.md).
- Real-game check on the owner's install (release v1.11.a, build 25615280) and a decompile of its mod loader: [notes/2026-10-17-real-game-loader.md](notes/2026-10-17-real-game-loader.md). Facts folded into the skills (hub R-20261017-9, C-20261017-3; modtools R-20261017-6, C-20261017-3 to -5). The big ones: the log is `Mods/mods.log` (the doc says mods.txt), `Mods/enabled.txt`, info.json allowed, `.json` cards only as a container, `IModEntrypoint` in the global namespace, mods target net48.
- Installed locally as `madnesswright@mark-local` (entry in the Ai folder's `.claude-plugin/marketplace.json`).
- Tests: 26 unit tests; value cases 3 of 3 runs on all six (one fail on run 2 of modtools outcome-1 fixed the net48 rule); baselines for all six; triggers and decoys 36/36 headless after a routing fix (first pass 30/36: the scenes skill never fired). Each TESTS.md has T-20261017-3 and -4.
- Banner `.github/images/banner.jpg` (Z-Image Turbo, seed 37, prompt beside it) and listing icon `.claude-plugin/icon.png` (a crop of the banner).

## Owed

1. **Beta launch check: done 2026-10-18 (see Session 3).** Was: A save backup exists (`~/mpn-backups/mpn-data-20261017-202940.zip`, 5 files with the career). The owner switches Steam to `releasebeta`, launches, enables `GreenBloodHank` (already deployed to `Mods/`) in the gear-icon menu, hits Hank, and reports the blood colour. Then run `mpn.py log`: confirm `game_version.mods_allowed: true`, record the real `mods.log` lines, check `loaded`, `applied_harmony` and the problem patterns against them, and log R-, C- and T- entries.
2. **Listings on hold** by the owner's ruling (2026-10-17): no Claude directory submission, and never awesome-copilot. Do not submit.
3. Scenes action-1 still does not separate skill from baseline: the baseline matched GUIDs against `Warehouse_PG`, which sits beside `Arena_PG`. Move `Arena_PG` into its own fixture project with no reference scene.
4. With the game on disk, modtools outcome-1 separates only weakly (the baseline read the metadata itself but double-patched by calling PatchAll in Initialize). Add an expectation on that.
5. The A/B verdict (`claude plugin eval` + `evergreen.py worth --record`) was not run: the evals are in evals.json, not plugin-eval case files. Try `evergreen.py eval-export` first.
6. Size budget is yellow: `mpn.py` is about 9,300 tokens (green up to 9,000), the hub description is 1,000 characters.
7. Headless SceneTools build route is untested (needs Unity 2019.2.6f1).
8. Evergreen refresh: skipped this session by ruling. The units are due 2026-11-16; the volatile releasebeta claim is due for recheck 2026-10-31.
9. **Requested mod:** swap characters and make everyone else unaware, as a Harmony mod and a live test of madnesswright-modtools. Plan and the game's hooks: [plans/2026-10-17-swap-and-unaware-mod.md](plans/2026-10-17-swap-and-unaware-mod.md). Needs `releasebeta`.

## Next step

The owner keeps the session-3 prompt in their private notes, since it names local paths. Without it: do Owed 1 once the owner has switched branch, then 3, 4 and 5.
