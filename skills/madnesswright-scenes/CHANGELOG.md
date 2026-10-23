# Changelog: madnesswright-scenes

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

Entry shape: `### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

### C-20261023-2 · 2026-10-23 · The last wave debuts nothing new; template columns carry the limits
- because: T-20261023-1 (first round: all three runs debuted two new roles in the final wave without a word; second round: one run set "Max attacking 3", one debuted two roles in wave 4), L-002
- files: references/encounter-design.md ("Wave and squad pacing"); references/level-plan-template.md (room and wave columns); evals/evals.json (outcome-2 expectation and evidence)
- The last wave uses only roles already met, and a brief with more new roles than waves is flagged with another wave or a move to the next stage offered. The column headers now say "one at most" and "1 or 2 attacking at once". The outcome-2 cap regex no longer matches a bare "simultaneously".

### C-20261023-1 · 2026-10-23 · Enemy roles, orchestration, wave pacing, finale and fairness rules; LEVEL-PLAN template
- because: books track 9 (Rogers, *Level Up!* 3rd ed., Levels 4 and 9 to 13 and 19, notes in the owner's book-learnings folder); T-20261023-1
- files: references/encounter-design.md (new sections "Enemy roles", "Orchestration order", "Wave and squad pacing", "Boss or finale room", "Fairness"; playtest checklist items 9 and 10); references/level-plan-template.md (new); SKILL.md (Step 4); evals/evals.json (outcome-2)
- Roles are decided per enemy card by watching it alone in a Playground arena: the game's cards sit inside compiled bundles, so no card-to-role mapping is asserted. The round length of 3 to 5 minutes is marked as a heuristic. The template holds a room table, a role-per-card table, a wave table, playtest results and a campaign beat chart whose music row musicwright-game can read.

### C-20261017-2 · 2026-10-17 · Route level and map requests to madnesswright-scenes
- because: T-20261017-4 (madnesswright-scenes 0/6 on its trigger runs; the hub and modtools took them)
- files: SKILL.md (description)
- Leads with 'make, fix or debug a level, map or stage', says there is no in-game editor, adds 'my Playground map doesn't show up' and 'I built the sobj', says level requests start here, and shortens the rule list to stay under 1,024 characters.

### C-20261017-1 · 2026-10-17 · Created as an evergreen unit
- because: user request
- files: SKILL.md, RESEARCH.md, LEARNINGS.md, evergreen.json (skills: also TESTS.md and evals/evals.json)
- Initial version. Tier `moderate`, interval 30d. See R-20261017-1 for the research basis; the first test run, for a skill, is logged in TESTS.md.
