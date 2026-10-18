# Tests: madnesswright

Test runs for [SKILL.md](SKILL.md). Cases live in `evals/evals.json`. A failure that taught something is a lesson in [LEARNINGS.md](LEARNINGS.md); a fix it caused is logged in [CHANGELOG.md](CHANGELOG.md) with `because: T-...`; research it triggered is in [RESEARCH.md](RESEARCH.md); counts and the failing list are in `evergreen.json` under `tests`. Rules: MAINTENANCE.md (testing section) and the plugin's `protocol/TESTING.md`.

A test passes on evidence (a tool call in the trace, a file, a marker, a log line), never on the transcript's claim that something was done.

Entry shape: `### T-YYYYMMDD-n · date · harness · env · passed/total`, then one line per failing case (`id · kind · class · what the evidence showed`), then `led to:` (L-, C-, R- ids or none). Newest first. Budget 150 lines; archive older runs to `TESTS-ARCHIVE.md`.

## Runs

### T-20261017-4 · 2026-10-17 · headless `claude -p` per run, plugin installed as madnesswright@mark-local · dev-machine · 12/12 (trigger and decoy, 3 runs each)
- First pass on 0.1.0: 30/36 across the plugin. Every miss was madnesswright-scenes: its four trigger runs went to the hub ("Start here") or to madnesswright-modtools ("my mobj doesn't load" caught "I built the sobj"). The runner also stopped at the first Skill call.
- After C-20261017-4 (routing lines in all three descriptions) and a runner that records up to three Skill calls: 36/36; the scenes skill fired first on all six of its runs; every decoy went to its sibling or to no skill.
- This skill: trigger-1, trigger-2 3/3 each; decoy-1 (level waves) went to madnesswright-scenes 3/3; decoy-2 (Valheim) no skill 3/3.
- led to: C-20261017-4

### T-20261017-3 · 2026-10-17 · evergreen-tester subagents · dev-machine · 4/4 (value cases, runs 2 and 3)
- action-1 (new fixture tests/fixtures/subtle-mod: misspelt Root, bare card in .json) · pass ×2 · `mpn.py check-mod` in the trace both runs; both faults named with fixes.
- action-1 baseline (no skill) · fail · read the files, found the misspelt Root, but guessed about hank.json (suggested renaming for consistency, never named the container rule). The fixture now separates skill from baseline.
- outcome-1 · pass ×2 · info.txt with Name and Description, container .json card `{"Character": [{"Filename": "Hank", "Root": "Hank", "Blood": {r,g,b,a}}]}`, check-mod 0 errors 0 warnings, no curly quotes.
- outcome-1 baseline (no skill) · fail · info.txt written as `Name: ...` lines, not JSON (check-mod: does not parse); no Root. The grading criteria were visible to the baseline, which may have helped its card format.
- led to: none

### T-20261017-2 · 2026-10-17 · evergreen-tester subagents · dev-machine · 2/2 (value cases, run 1 of 3)
- action-1 · pass · ran `mpn.py check-mod` on tests/fixtures/broken-mod; reply named the curly quotes and the trailing comma.
- action-1 baseline (no skill) · also found both faults by reading the JSON: the case does not discriminate. Replace the fixture with a misspelt Root or a shorthand card without Type in a .json file.
- outcome-1 · pass · GreenBloodHank with info.txt and a strict .json Character card; check-mod 0 errors; no curly quotes; the Blood shape and Hank Filename flagged "confirm in MCM" in a README.
- Trigger and decoy cases not run: the plugin was not installed in the session. Owed.
- led to: none

### T-20261017-1 · 2026-10-17 · not yet run · skill · 0/0
- Suite scaffolded; no run recorded. Write the cases in `evals/evals.json` (at least two trigger prompts, two decoys, one action case with evidence, one outcome case), run the baseline without the skill, then run with it (`evergreen-test`).
- led to: none
