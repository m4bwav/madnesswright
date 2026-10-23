# Learnings: madnesswright-scenes

Procedural lessons for [SKILL.md](SKILL.md). Research findings live in [RESEARCH.md](RESEARCH.md); every change is logged in [CHANGELOG.md](CHANGELOG.md); test runs in [TESTS.md](TESTS.md); state in `evergreen.json`. Format and write-time gate: MAINTENANCE.md (LEARNINGS-FORMAT). Retired entries go to LEARNINGS-ARCHIVE.md with a reason.

Write an entry the moment a real signal happens: a user correction, the same error twice, a discovered workaround, an environment fact, a stated preference, a failed test or a failure in use. Check existing entries first, by meaning (`evergreen.py search "<the lesson>" --kinds learnings` finds near-duplicates in every registered unit): add / update / retire / none. Trigger and Hypothesis are required. Promote after three confirmations; retire when harmful > helpful.

## Active

### L-001 · 2026-10-17 · A sibling hub that says "start here" eats the specialist's triggers
- Trigger: T-20261017-4, all six scenes trigger runs went to madnesswright or madnesswright-modtools on 0.1.0
- Hypothesis: the hub's broad "mod Project Nexus" and "Start here" outrank a specialist description that opens with tool names; ".sobj" read as close to ".mobj"
- Rule: open the description with the user's words (level, map, stage) and say "requests start here"; give each sibling an explicit sentence routing levels here; re-run every sibling's triggers after any description edit, with a runner that records more than the first Skill call
- Evidence: C-20261017-2, T-20261017-4 (36/36 after), confirmed 2026-10-17
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-17

### L-002 · 2026-10-23 · A rule the brief cannot meet gets broken silently; a limit in the column header holds
- Trigger: T-20261023-1, outcome-2 asked for five enemy types in five waves; 3/3 runs debuted two in the finale without a word; after a prose rule, 1 of 3 still set "Max attacking 3"
- Hypothesis: the model satisfies the brief first and treats prose rules as advice; a column it must fill states the limit at the moment it writes the number
- Rule: give each pacing rule an escape ("say so and offer another wave") and put the number limits into the template's column headers
- Evidence: C-20261023-2, T-20261023-1 round 3 (3/3)
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-23

<!-- Example (delete once you have a real entry):
### L-001 · 2026-10-17 · One-line lesson in plain words
- Trigger: what happened, with dates or counts
- Hypothesis: why
- Rule: the shortest instruction that prevents the trigger
- Evidence: C-20261017-1, T-20261017-1, confirmed 2026-10-17
- Scope: skill | repo:<slug> | env:<name> | global
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-17
-->
