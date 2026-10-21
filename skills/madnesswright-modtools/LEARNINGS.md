# Learnings: madnesswright-modtools

Procedural lessons for [SKILL.md](SKILL.md). Research findings live in [RESEARCH.md](RESEARCH.md); every change is logged in [CHANGELOG.md](CHANGELOG.md); test runs in [TESTS.md](TESTS.md); state in `evergreen.json`. Format and write-time gate: MAINTENANCE.md (LEARNINGS-FORMAT). Retired entries go to LEARNINGS-ARCHIVE.md with a reason.

Write an entry the moment a real signal happens: a user correction, the same error twice, a discovered workaround, an environment fact, a stated preference, a failed test or a failure in use. Check existing entries first, by meaning (`evergreen.py search "<the lesson>" --kinds learnings` finds near-duplicates in every registered unit): add / update / retire / none. Trigger and Hypothesis are required. Promote after three confirmations; retire when harmful > helpful.

## Active

### L-001 · 2026-10-18 · Grep the callers before a mod changes game state
- Trigger: the first live mod (swap control to any character) threw a NullReferenceException inside the game's own `KillMe` when a taken-over Playground spawn died: the game's death-swap read a squad the spawn did not have. The owner reported "when they die I can't swap". 2026-10-18.
- Hypothesis: game code is written for the states the game itself reaches; a mod that reaches new ones (no squad, enemy body, another room) meets unguarded fields in follow-up code it never calls directly.
- Rule: before deploying, grep the decompile for callers of every game method the mod uses and of the death, room and squad paths; add a Prefix guard where the new state breaks them; ask for an edge-case playtest.
- Evidence: C-20261018-1, T-20261018-1, T-20261018-2 (the 0.2.0 guard held), confirmed 2026-10-18
- Scope: skill
- Status: active · helpful 2 · harmful 0 · last_confirmed 2026-10-18

### L-002 · 2026-10-18 · Change AI state through the game's calls, not its fields
- Trigger: the swap mod's F5 failed on characters mid-fight after F6 had set `myKillTarget = null` by hand; `Player.log` showed a NullReferenceException in `AI_TargetClaimer.SetPriority` from `Controller_Base.LostConnection` on every failed swap (beta v2.20.a, 2026-10-18).
- Hypothesis: game state is often stored twice (a field and a registry list); writing one side leaves the other stale, and the game's own code later trips over the mismatch.
- Rule: when a mod changes targets, controllers, squads or rooms, find the game method that does it (grep where the game itself does the same change, e.g. `KillMe`) and call that; release a controller's claims before moving it.
- Evidence: C-20261018-2, T-20261018-2 (0.3.0: no exception in 7 swaps), confirmed 2026-10-18
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-18

### L-003 · 2026-10-21 · A key in the decompile is a clash only if it is read in play
- Trigger: the second mod's key check (2026-10-21) found F1 to F4, F9 to F11, P and Delete all read somewhere in the game's code, which looked like seven forced key moves; reading each caller showed they run only in debug builds (`Dev_Control`), with the `-devtools` launch option (`ModTestTools`), or inside the profile window while it is open. Only CapsLock (autowalk, a rebindable default) is read in normal play.
- Hypothesis: shipped games keep their developer consoles and menu shortcuts in the build; a grep counts them all, so the answer depends on when each caller runs, not on whether the key appears.
- Rule: check keys with `decomp.py keys` and treat only its `clashes` as candidates; open each listed file and ask when it runs before moving a key the user asked for. Keep the requester's keys where nothing in play reads them.
- Evidence: C-20261021-1, T-20261021-1 (all eight keys kept; no clash seen in play)
- Scope: skill
- Status: active · helpful 2 · harmful 0 · last_confirmed 2026-10-21

### L-004 · 2026-10-21 · Mod data files go outside the mod folder
- Trigger: the research pass for saving Playground scenes suggested `Mods/<mod>/Scenes/*.json`; the game's loader (session 2 notes) reads every `.json` and `.txt` in a mod folder as a MadCard, so each saved scene would load as a broken card (2026-10-21).
- Hypothesis: modding guides and agents treat a mod's own folder as private storage; in this game it is card input.
- Rule: a mod that writes files puts them under `Application.persistentDataPath/<ModName>/`, never inside `Mods/`, and never under `<SteamID>/Careers` (the game deletes Playground careers there).
- Evidence: C-20261021-1, notes 2026-10-17-real-game-loader.md
- Scope: skill
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-21

<!-- Example (delete once you have a real entry):
### L-001 · 2026-10-17 · One-line lesson in plain words
- Trigger: what happened, with dates or counts
- Hypothesis: why
- Rule: the shortest instruction that prevents the trigger
- Evidence: C-20261017-1, T-20261017-1, confirmed 2026-10-17
- Scope: skill | repo:<slug> | env:<name> | global
- Status: active · helpful 1 · harmful 0 · last_confirmed 2026-10-17
-->
