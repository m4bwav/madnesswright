# Changelog: madnesswright

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

Entry shape: `### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

### C-20261017-3 · 2026-10-17 · Loader facts from the real game: mods.log, enabled.txt, info.json, card formats
- because: R-20261017-9
- files: SKILL.md (description, Step 2, Step 3, Step 5, Step 6), references/troubleshooting.md (rewritten), references/madcards.md (What a card is, Two file formats, info.txt), scripts/mpn.py (where, check-mod, log), tests/test_mpn.py
- The mod log is `Mods/mods.log`; `where` and `log` report it, `enabled.txt` and the build from Player.log; `log` parses the enabled-mod and Harmony lines and drops clean-launch noise. `check-mod` accepts info.json, requires Description, errors on bare cards in `.json` and unknown `.txt` Types, warns on unknown `.json` lists and near-miss Roots, and only warns on curly double quotes in `.txt` cards. Colour syntax confirmed.

### C-20261017-2 · 2026-10-17 · where reports an unfinished download; shorter description
- because: T-20261017-2 (a tester found a Steam manifest with 0 bytes downloaded that `where` reported as a normal install), budget.py yellow on description length
- files: scripts/mpn.py (find_install, cmd_where), SKILL.md (description)
- `where` now returns `fully_installed` and `download` from StateFlags and the byte counts, and says to finish the install first; the description dropped the studio and app id to get under 950 characters.

### C-20261017-1 · 2026-10-17 · Created as an evergreen unit
- because: user request
- files: SKILL.md, RESEARCH.md, LEARNINGS.md, evergreen.json (skills: also TESTS.md and evals/evals.json)
- Initial version. Tier `moderate`, interval 30d. See R-20261017-1 for the research basis; the first test run, for a skill, is logged in TESTS.md.
