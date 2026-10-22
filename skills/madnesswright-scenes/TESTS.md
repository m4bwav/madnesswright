# Tests: madnesswright-scenes

Test runs for [SKILL.md](SKILL.md). Cases live in `evals/evals.json`. A failure that taught something is a lesson in [LEARNINGS.md](LEARNINGS.md); a fix it caused is logged in [CHANGELOG.md](CHANGELOG.md) with `because: T-...`; research it triggered is in [RESEARCH.md](RESEARCH.md); counts and the failing list are in `evergreen.json` under `tests`. Rules: MAINTENANCE.md (testing section) and the plugin's `protocol/TESTING.md`.

A test passes on evidence (a tool call in the trace, a file, a marker, a log line), never on the transcript's claim that something was done.

Entry shape: `### T-YYYYMMDD-n · date · harness · env · passed/total`, then one line per failing case (`id · kind · class · what the evidence showed`), then `led to:` (L-, C-, R- ids or none). Newest first. Budget 150 lines; archive older runs to `TESTS-ARCHIVE.md`.

## Runs

### T-20261022-2 · 2026-10-22 · `claude plugin eval` 2.1.281 (cases from `evergreen.py eval-export --out evals/<skill>` in a scratch copy of the plugin), native Windows · 4/6 cases, both arms, 3 runs each
- trigger-1, trigger-2 · pass 3/3 with the plugin, 0/3 without (delta +1); decoy-1, decoy-2 · quiet 3/3 in both arms.
- action-1 · harness · fail in both arms · the exported case grants no shell by default (0 Bash calls); with `--allow-tools Bash Write Edit` every run was refused: "sandbox required but unavailable: the Windows sandbox is not active on this session (feature gate off)".
- outcome-1 · harness · fail in both arms · exported with read-only tools, so no run could write LEVEL-PLAN.md (the with-plugin run wrote the plan into its reply instead).
- `evergreen.py worth --record` on these results: UNPROVEN (every value case failed in both arms), written to evergreen.json. Value cases need WSL2 or `worth --ab` (`claude -p`) on this machine. Cost $5.01, 216 s.
- led to: none

### T-20261022-1 · 2026-10-22 · evergreen-tester subagents · dev-machine · 1/1 (action-1 run 1, plus baseline)
- action-1 · pass · fixture moved to its own project `tests/fixtures/arena-project` (scene, prefab Library, TagManager; no reference scene). `mpn.py scene-lint` in the trace; S01 and S05 reported with fixes; 2 tool calls.
- action-1 baseline (no skill) · pass on correctness · still found the missing Player_Start and Zone_Camera under Characters, by matching prefab GUIDs to the Library .meta files and reading Room_Layout's child names (4 reads); called Characters vs Waypoints a guess and added an unverified Event System note. The case separates on cost, rule IDs and next steps (build, check-mod, deploy), not on correctness. Untested elsewhere.
- led to: none

### T-20261017-4 · 2026-10-17 · headless `claude -p` per run, plugin installed as madnesswright@mark-local · dev-machine · 12/12 (trigger and decoy, 3 runs each)
- First pass on 0.1.0: 30/36 across the plugin. Every miss was madnesswright-scenes: its four trigger runs went to the hub ("Start here") or to madnesswright-modtools ("my mobj doesn't load" caught "I built the sobj"). The runner also stopped at the first Skill call.
- After C-20261017-2 (routing lines in all three descriptions) and a runner that records up to three Skill calls: 36/36; the scenes skill fired first on all six of its runs; every decoy went to its sibling or to no skill.
- This skill: trigger-1, trigger-2 0/3 each before the fix, 3/3 each after; decoy-1 (Hank's health) went to madnesswright or modtools 3/3; decoy-2 (Unity 6 platformer) no skill 3/3.
- led to: C-20261017-2, L-001

### T-20261017-3 · 2026-10-17 · evergreen-tester subagents · dev-machine · 4/4 (value cases, runs 2 and 3)
- action-1 (new fixture Arena_PG.unity: no Player_Start, Zone_Camera under Characters) · pass ×2 · `mpn.py scene-lint` in the trace; S01 and S05 reported with fixes.
- action-1 baseline (no skill) · also found both, by matching prefab GUIDs to .meta files and comparing with Warehouse_PG. The case still does not separate skill from baseline while a correct sibling scene sits beside it; the skill's edge is two tool calls against a dozen. Next fixture: no reference scene in the folder.
- outcome-1 · pass ×2 · LEVEL-PLAN.md with one entry per room, at most one new enemy type per room, Squad Spawner (run 3 with a Zone_Spawner trigger), filled playtest checklist, `_PG` and one Player_Start under Characters.
- outcome-1 baseline (no skill) · fail · plan had rooms and waves but no Squad Spawner or Zone_Spawner by name, an unfilled checklist, no `_PG`, no Characters parent, no SceneTools.
- led to: none

### T-20261017-2 · 2026-10-17 · evergreen-tester subagents · dev-machine · 2/2 (value cases, run 1 of 3)
- action-1 · pass · ran `mpn.py scene-lint`; reported S03 and S13 with fixes. Note: the script exits 0 with errors unless `--strict` is passed (by design).
- action-1 baseline (no skill) · found the rotation and the empty label by reading the YAML, but called the rotation a style choice, not a rule that breaks the room: partial. Harder fixture owed (Zone_Camera under the wrong parent, a missing Player_Start).
- outcome-1 · pass · LEVEL-PLAN.md with one row per room, at most one new enemy type per room, Squad Spawner for the wave room, filled playtest checklist, _PG and Player_Start under Characters named; the grunt Filename left as a placeholder to confirm.
- Trigger and decoy cases not run. Owed.
- led to: none

### T-20261017-1 · 2026-10-17 · not yet run · skill · 0/0
- Suite scaffolded; no run recorded. Write the cases in `evals/evals.json` (at least two trigger prompts, two decoys, one action case with evidence, one outcome case), run the baseline without the skill, then run with it (`evergreen-test`).
- led to: none
