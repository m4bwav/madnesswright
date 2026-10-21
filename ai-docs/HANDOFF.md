# Handoff

State on 2026-10-21, session 5 (Playground Director 0.4.0 playtested and on the Workshop; plugin v0.1.5 tagged and released).

## Session 5

- Request: eight new keys for the swap mod's player (F1 clear, F2 freeze, F3 patrols, F4 idle animations, F9 guard, F10 alert cycle, F11 saved scenes). Decision: grow the existing mod, renamed Playground Director, one component per feature ([decisions/2026-10-21-update-swap-mod-not-new.md](decisions/2026-10-21-update-swap-mod-not-new.md)). Plan with rulings and the player's checklist: [plans/2026-10-21-playground-director.md](plans/2026-10-21-playground-director.md). Game hooks: [notes/2026-10-21-director-mod-research.md](notes/2026-10-21-director-mod-research.md).
- No key moved: none of the requested keys is read in normal play (CapsLock is autowalk; read only while paused).
- Playtested on the owner's PC (modtools T-20261021-1: F1 to F4, F10 and F5 to F8 seen in Player.log, no exception; F9 guard and F11 scenes not yet exercised, confirm next report). Published as an update of public item 3822329165 (title "Playground Director (was Swap And Unaware)", new preview), still public, 28 subscribers. Wiki worked example updated.
- Plugin v0.1.5 tagged and released (the owner's go-ahead; 0.1.3 and 0.1.4 are folded into it): `scripts/decomp.py` (decompile, keys, find, callers; 5 tests in CI), `references/game-recipes.md`, L-003, L-004.
- Likely cause of the 0.3.0 "teammate turned on me" report: Playground spawns are `Factions.None` squads, hostile to everyone outside their squad. Budget: still yellow (mpn.py 9,500 tokens; installed skills folder 205 KB, green up to 200).

## Session 4

- Swap mod 0.3.0 fixed the mid-fight take-over failure (duplicate AI target claimants, L-002). The owner's playtest passed on two PCs, the second installed from the Workshop (modtools T-20261018-2). One unconfirmed report: a grunt added to the player's own team turned on the player once; the owner chose to leave it.
- Source lives in [examples/SwapUnaware](../examples/SwapUnaware/README.md) and builds there (`pwsh ./pack.ps1 -Version X`); `mpn-mods/SwapUnaware` outside the repo holds old zips only.
- Public Workshop item 3822329165 (https://steamcommunity.com/sharedfiles/filedetails/?id=3822329165), uploaded with [examples/WorkshopUpload](../examples/WorkshopUpload/README.md) through the owner's logged-in Steam. Its update path is untested. Building it needs the uploader zip's Facepunch DLLs in `lib/`; Claude Code's auto mode refused that build until the owner approved it.
- Loader diff (release vs beta) cannot be done: the install is on the beta.
- Plugin 0.1.4 merged (PRs #2, #3, #4), not tagged. Tagging waits for the owner.
- Still owed: 3, 4, 5 below (eval fixes) and the size budget (mpn.py ~9,500 tokens, tracked KB yellow).

## Session 3

- First beta launch passed (build 25615196, green blood: [notes/2026-10-18-beta-launch.md](notes/2026-10-18-beta-launch.md)); swap mod 0.1.0 and 0.2.0 built and fixed (details in [log.md](log.md)); 0.1.3 merged (PR #1). Loader diff release vs beta not possible while on the beta.

## Done

- Public: https://github.com/m4bwav/madnesswright (topics set, private vulnerability reporting on, CI green on Windows, macOS and Linux with Python 3.9 and 3.14). Releases v0.1.1 and v0.1.2 (https://github.com/m4bwav/madnesswright/releases/tag/v0.1.2) from `.github/workflows/release.yml`. 0.1.0 was never tagged.
- Wiki live at https://github.com/m4bwav/madnesswright/wiki (10 pages, names 0.1.2): how it was made and how to update it in [notes/2026-10-17-github-wiki.md](notes/2026-10-17-github-wiki.md).
- Real-game check on the owner's install (release v1.11.a, build 25615280) and a decompile of its mod loader: [notes/2026-10-17-real-game-loader.md](notes/2026-10-17-real-game-loader.md). Facts folded into the skills (hub R-20261017-9, C-20261017-3; modtools R-20261017-6, C-20261017-3 to -5). The big ones: the log is `Mods/mods.log` (the doc says mods.txt), `Mods/enabled.txt`, info.json allowed, `.json` cards only as a container, `IModEntrypoint` in the global namespace, mods target net48.
- Installed locally as `madnesswright@mark-local` (entry in the Ai folder's `.claude-plugin/marketplace.json`).
- Tests: 26 unit tests; value cases 3 of 3 runs on all six (one fail on run 2 of modtools outcome-1 fixed the net48 rule); baselines for all six; triggers and decoys 36/36 headless after a routing fix (first pass 30/36: the scenes skill never fired). Each TESTS.md has T-20261017-3 and -4.
- Banner `.github/images/banner.jpg` (Z-Image Turbo, seed 37, prompt beside it) and listing icon `.claude-plugin/icon.png` (a crop of the banner).

## Owed

1. Beta launch check: done 2026-10-18 (Session 3).
2. **Listings on hold** by the owner's ruling (2026-10-17): no Claude directory submission, and never awesome-copilot. Do not submit.
3. Scenes action-1 still does not separate skill from baseline: the baseline matched GUIDs against `Warehouse_PG`, which sits beside `Arena_PG`. Move `Arena_PG` into its own fixture project with no reference scene.
4. With the game on disk, modtools outcome-1 separates only weakly (the baseline read the metadata itself but double-patched by calling PatchAll in Initialize). Add an expectation on that.
5. The A/B verdict (`claude plugin eval` + `evergreen.py worth --record`) was not run: the evals are in evals.json, not plugin-eval case files. Try `evergreen.py eval-export` first.
6. Size budget is yellow: `mpn.py` is about 9,300 tokens (green up to 9,000), the hub description is 1,000 characters.
7. Headless SceneTools build route is untested (needs Unity 2019.2.6f1).
8. Evergreen refresh: skipped this session by ruling. The units are due 2026-11-16; the volatile releasebeta claim is due for recheck 2026-10-31.
9. **Requested mod:** swap characters and make everyone else unaware, as a Harmony mod and a live test of madnesswright-modtools. Plan and the game's hooks: [plans/2026-10-17-swap-and-unaware-mod.md](plans/2026-10-17-swap-and-unaware-mod.md). Needs `releasebeta`.

10. Playground Director: confirm F9 guard and F11 scenes in the next play report.

## Next step

The owner keeps the session-3 prompt in their private notes, since it names local paths. Without it: do Owed 1 once the owner has switched branch, then 3, 4 and 5.
