# Changelog: madnesswright

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

Entry shape: `### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

### C-20261025-1 · 2026-10-25 · Workshop updates get real change notes
- because: user request (three Director updates showed only "Update" on the Change Notes tab)
- files: SKILL.md (Step 6: share it), examples/WorkshopUpload (change-notes file, refuses an update without one)
- Step 6 says every update carries the version and what changed for the player, and where to write it with or without the bundled uploader.

### C-20261018-2 · 2026-10-18 · Workshop uploader's new home and its quirks
- because: R-20261018-2
- files: SKILL.md (Workshop), references/sources.md
- Points at examples/WorkshopUpload, a no-prompt uploader. The uploader repo moved; the skill names the dotted content-folder quirk and that uploads go through the logged-in Steam client.

### C-20261018-1 · 2026-10-18 · Gear-icon check, redeploy while running, log stacks and card lines
- because: R-20261018-1 (first beta launch), T-20261018-1 in madnesswright-modtools (a crash whose cause was in the stack frames `log` did not show)
- files: SKILL.md (Step 5, enabling), scripts/mpn.py (log: `stack` on exception lines, `mods_log.cards` from OVERRIDE/FAILED lines, the from-bytes assembly line is noise), tests/test_mpn.py (real beta 2.20.a log shapes)
- The owner looked for the gear icon on the release build, which deletes it; the skill now says so. `log` shows the frames that name the method that threw and which cards the game applied.


### C-20261017-5 · 2026-10-17 · Doc corrections found by the wiki run
- because: the wiki verification run of 2026-10-17 (ai-docs/notes/2026-10-17-github-wiki.md, inaccuracies 1, 3, 4)
- files: scripts/mpn.py (docstring), references/troubleshooting.md (item 2), references/madcards.md (What a card is)
- `--strict` goes before the command name; check-mod does not detect a nested folder; the loader also accepts Stage_CAMPAIGN and an Item list.

### C-20261017-4 · 2026-10-17 · Route level and map requests to madnesswright-scenes
- because: T-20261017-4 (madnesswright-scenes 0/6 on its trigger runs; the hub and modtools took them)
- files: SKILL.md (description)
- The routing sentence now sends any level, map, Playground scene, .sobj, stage, arena or campaign request to madnesswright-scenes; `.json` cards are described as the container format and the log as Mods/mods.log.

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
