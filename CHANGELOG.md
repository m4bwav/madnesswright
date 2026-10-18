# Changelog

Plugin versions. Skill-level changes and their reasons are in each skill's own CHANGELOG.md.

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
