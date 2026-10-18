# Handoff

State on 2026-10-17, end of the creation session.

## Done

- Plugin built: three skills, `mpn.py` (where, backup, check-mod, deploy, log, scene-lint), 18 unit tests, structure checker, size budget, CI workflow, README, AGENTS.md, SECURITY.md, LICENSE.
- Creation pass research logged in each skill's RESEARCH.md (R-20261017-*). Official docs read in full: Modding Overview, SceneTools Overview, Event System Overview.
- `scene-lint` verified on both official sample scenes (clean) and a mutated copy (caught S03 and S13).
- Eval suites written. Run 1 of the six action and outcome cases passed with the skill; three action baselines run (see each TESTS.md, T-20261017-2).

## Owed

1. Trigger and decoy cases have not been run: they need the plugin installed and a fresh session (or headless `claude -p` runs).
2. Runs 2 and 3 of each value case; the hub and scenes action fixtures are too easy (baselines found the faults by plain reading). Make harder fixtures: a card with a misspelt Root, a Zone_Camera under the wrong parent, a scene with two Player_Starts inside a nested room.
3. Real-game verification once the install finishes (Steam had queued it on 2026-10-17, 0 of 2.26 GB): run `mpn.py where`, confirm the Player.log path, list `Managed/` (netstandard.dll? 0Harmony.dll?), decompile and record IModEntrypoint's namespace, look at a real `mods.txt`, switch to releasebeta after `mpn.py backup`.
4. Launch set before going public: README banner (ComfyUI, 1536x640, `.github/images/banner.jpg`), GitHub repository, wiki (wikiwright), topics, release v0.1.0. Waiting for the owner's go-ahead to publish.
5. Headless SceneTools build route is untested (needs Unity 2019.2.6f1).

## Next step

Read [log.md](log.md), then do Owed 3 if the game is installed, else Owed 1.
