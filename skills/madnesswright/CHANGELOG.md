# Changelog: madnesswright

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

Entry shape: `### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

### C-20261017-2 · 2026-10-17 · where reports an unfinished download; shorter description
- because: T-20261017-2 (a tester found a Steam manifest with 0 bytes downloaded that `where` reported as a normal install), budget.py yellow on description length
- files: scripts/mpn.py (find_install, cmd_where), SKILL.md (description)
- `where` now returns `fully_installed` and `download` from StateFlags and the byte counts, and says to finish the install first; the description dropped the studio and app id to get under 950 characters.

### C-20261017-1 · 2026-10-17 · Created as an evergreen unit
- because: user request
- files: SKILL.md, RESEARCH.md, LEARNINGS.md, evergreen.json (skills: also TESTS.md and evals/evals.json)
- Initial version. Tier `moderate`, interval 30d. See R-20261017-1 for the research basis; the first test run, for a skill, is logged in TESTS.md.
