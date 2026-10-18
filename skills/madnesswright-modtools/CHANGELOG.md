# Changelog: madnesswright-modtools

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

Entry shape: `### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

### C-20261017-3 · 2026-10-17 · Harmony loader facts from the real game
- because: R-20261017-6
- files: SKILL.md (Step 5), references/harmony.md (Source, info.txt keys, Entry class, Project file, Framework rule, Failure modes)
- Interface namespace (global), net472 for the current build, the shipped 0Harmony 2.4.2, how blank HarmonyIDs and extra DLLs behave, the exact log lines, and what an Initialize failure or disable does.

### C-20261017-2 · 2026-10-17 · Shorter description
- because: budget.py yellow on description length
- files: SKILL.md (description)
- Dropped the clause about proving loading from mods.txt from the description (the body still does it).

### C-20261017-1 · 2026-10-17 · Created as an evergreen unit
- because: user request
- files: SKILL.md, RESEARCH.md, LEARNINGS.md, evergreen.json (skills: also TESTS.md and evals/evals.json)
- Initial version. Tier `moderate`, interval 30d. See R-20261017-1 for the research basis; the first test run, for a skill, is logged in TESTS.md.
