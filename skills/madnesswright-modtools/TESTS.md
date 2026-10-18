# Tests: madnesswright-modtools

Test runs for [SKILL.md](SKILL.md). Cases live in `evals/evals.json`. A failure that taught something is a lesson in [LEARNINGS.md](LEARNINGS.md); a fix it caused is logged in [CHANGELOG.md](CHANGELOG.md) with `because: T-...`; research it triggered is in [RESEARCH.md](RESEARCH.md); counts and the failing list are in `evergreen.json` under `tests`. Rules: MAINTENANCE.md (testing section) and the plugin's `protocol/TESTING.md`.

A test passes on evidence (a tool call in the trace, a file, a marker, a log line), never on the transcript's claim that something was done.

Entry shape: `### T-YYYYMMDD-n · date · harness · env · passed/total`, then one line per failing case (`id · kind · class · what the evidence showed`), then `led to:` (L-, C-, R- ids or none). Newest first. Budget 150 lines; archive older runs to `TESTS-ARCHIVE.md`.

## Runs

### T-20261017-2 · 2026-10-17 · evergreen-tester subagents · dev-machine · 2/2 (value cases, run 1 of 3)
- action-1 · pass · looked for the install, `Managed/` and `Assembly-CSharp.dll` and for ilspycmd before writing code (the game's files were not downloaded); the HarmonyPatch target was written as a labelled placeholder; info.txt had AssemblyPath, HarmonyID, EntryType.
- action-1 baseline (no skill) · fail · no lookup of Assembly-CSharp or Managed; used BepInEx 5 (`BaseUnityPlugin`) instead of the game's own loader; no info.txt. The skill made the difference.
- outcome-1 · pass · csproj with Private=false references from MPN_MANAGED and a provisional netstandard2.0 with the rule in a comment; IModEntrypoint entry class logging the HarmonyID; no game method named; check-mod's only error was the expected missing DLL.
- Trigger and decoy cases not run. Owed.
- led to: none

### T-20261017-1 · 2026-10-17 · not yet run · skill · 0/0
- Suite scaffolded; no run recorded. Write the cases in `evals/evals.json` (at least two trigger prompts, two decoys, one action case with evidence, one outcome case), run the baseline without the skill, then run with it (`evergreen-test`).
- led to: none
