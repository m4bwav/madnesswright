# Changelog

Plugin versions. Skill-level changes and their reasons are in each skill's own CHANGELOG.md.

## 0.1.6 · unreleased

- `examples/SwapUnaware` 0.5.0: F11 scenes in story levels, arenas, custom stages and hubs, not only the Playground. Loading matches the characters already in the level to the saved ones and puts them back, respawns the missing (serial numbers re-registered, wave units back in the wave), removes only unscripted extras, and moves you back to your saved room. Playground scenes from another Playground stage load that stage first. Scene files version 2; 0.4.0 files still load. Deployed, playtest pending.
- madnesswright-modtools: `references/game-recipes.md` gains "Levels, rooms and the career save".

## 0.1.5 · 2026-10-21

- madnesswright-modtools: new `scripts/decomp.py` (decompile, `keys` for which keys the game reads in play, `find` and `callers` for one-line member lookups) with `tests/test_decomp.py` in CI; new `references/game-recipes.md` (freeze, remove, playable roster, teams, AI states, idle poses, walking to a point, blocking the attack click, IMGUI windows, saving a scene); lessons L-003 (a key is a clash only if read in play) and L-004 (mod data outside `Mods/`).
- `examples/SwapUnaware` 0.4.0, now Playground Director: F1 clear, F2 freeze, F3 patrols, F4 idle animations, F9 guard, F10 alert cycle, F11 saved scenes, one component per feature. Playtested on the owner's PC (F9 and F11 not yet exercised) and published as an update of Workshop item 3822329165, which stayed public; `examples/WorkshopUpload`'s update path now proven.

## 0.1.4 · 2026-10-18

- madnesswright: the Workshop uploader moved to placenamehereig/MPN-Workshop-Uploader; content folder needs a dot in its name; the example mod is on the Workshop (item 3822329165); `examples/WorkshopUpload` publishes or updates an item from arguments, with no prompts.
- SwapUnaware 0.3.0 playtested on two PCs (modtools T-20261018-2).

- New `examples/SwapUnaware`: a complete Harmony mod (take control of any character, everyone else unaware) with its build and pack scripts and the bugs its playtests found; linked from the README and madnesswright-modtools.
- madnesswright-modtools: release an AI's target claims through `AI_TargetClaimer.UnclaimMyTarget` before moving a controller; never null `myKillTarget` by hand (L-002).

## 0.1.3 · 2026-10-18

- First beta launch (v2.20.a, build 25615196) and the first live Harmony mod built with madnesswright-modtools.
- madnesswright: no gear icon means not on the beta (the release build deletes the button); `MPN deploy --force` works while the game runs.
- `mpn.py log`: exception lines carry their `stack` frames, `mods_log.cards` lists OVERRIDE and FAILED card lines, and the from-bytes assembly line is no longer flagged.
- madnesswright-modtools: patch-free mods (a MonoBehaviour host), grep the callers of every game method a mod leans on and guard states the game never reaches, room-scoped game queries; first LEARNINGS entry.

## 0.1.2 · 2026-10-17

- Root `plugin.json` (Agent Plugins 1.0 shape) for Copilot CLI installs; kept equal to `.claude-plugin/plugin.json`.
- madnesswright-modtools Step 5.1 names the `Madness Project Nexus 2` install folder (testers guessed wrong twice).
- Doc corrections from the wiki run: `--strict`, `--data-dir` and `--steam` go before the command; mpn.py runs on Windows, macOS and Linux; modtools RESEARCH says net48; troubleshooting no longer claims check-mod detects nested folders.

## 0.1.1 · 2026-10-17

- Checked against the real game (release v1.11.a, build 25615280) and its decompiled mod loader: the mod log is `Mods/mods.log` (the official doc says `mods.txt`), the enabled list is `Mods/enabled.txt`, info can be `info.txt` or `info.json` with Name and Description required, `.json` cards only as a container, `.txt` Types case-sensitive, `IModEntrypoint` in the global namespace, Harmony 2.4.2 shipped, mods target net48.
- `mpn.py`: `where` and `log` report the build from Player.log, the enabled list, loaded mods and applied Harmony patches; `check-mod` follows the loader's rules and flags near-miss Roots.
- Harder eval fixtures, trigger routing between the three skills, release workflow, Dependabot for actions, README banner.

## 0.1.0 · 2026-10-17

- First version: `madnesswright` (mods, MadCards, branch, deploy, log, Workshop), `madnesswright-scenes` (SceneTools levels, stages, campaigns, arena, encounter design, playtest), `madnesswright-modtools` (ModTools MadObjects, Harmony assemblies).
- `mpn.py`: where, backup, check-mod, deploy, log, scene-lint (13 rules from the official SceneTools Overview).
- Creation pass research, eval suites, structure checks, size budget and CI.
