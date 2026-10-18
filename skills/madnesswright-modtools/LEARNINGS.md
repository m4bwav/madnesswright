# Learnings: madnesswright-modtools

Procedural lessons for [SKILL.md](SKILL.md). Research findings live in [RESEARCH.md](RESEARCH.md); every change is logged in [CHANGELOG.md](CHANGELOG.md); test runs in [TESTS.md](TESTS.md); state in `evergreen.json`. Format and write-time gate: MAINTENANCE.md (LEARNINGS-FORMAT). Retired entries go to LEARNINGS-ARCHIVE.md with a reason.

Write an entry the moment a real signal happens: a user correction, the same error twice, a discovered workaround, an environment fact, a stated preference, a failed test or a failure in use. Check existing entries first, by meaning (`evergreen.py search "<the lesson>" --kinds learnings` finds near-duplicates in every registered unit): add / update / retire / none. Trigger and Hypothesis are required. Promote after three confirmations; retire when harmful > helpful.

## Active

### L-001 · 2026-10-18 · Grep the callers before a mod changes game state
- Trigger: the first live mod (swap control to any character) threw a NullReferenceException inside the game's own `KillMe` when a taken-over Playground spawn died: the game's death-swap read a squad the spawn did not have. The owner reported "when they die I can't swap". 2026-10-18.
- Hypothesis: game code is written for the states the game itself reaches; a mod that reaches new ones (no squad, enemy body, another room) meets unguarded fields in follow-up code it never calls directly.
- Rule: before deploying, grep the decompile for callers of every game method the mod uses and of the death, room and squad paths; add a Prefix guard where the new state breaks them; ask for an edge-case playtest.
- Evidence: C-20261018-1, T-20261018-1, confirmed 2026-10-18
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-18


<!-- Example (delete once you have a real entry):
### L-001 · 2026-10-17 · One-line lesson in plain words
- Trigger: what happened, with dates or counts
- Hypothesis: why
- Rule: the shortest instruction that prevents the trigger
- Evidence: C-20261017-1, T-20261017-1, confirmed 2026-10-17
- Scope: skill | repo:<slug> | env:<name> | global
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-17
-->
