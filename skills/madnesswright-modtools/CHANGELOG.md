# Changelog: madnesswright-modtools

Every change to [SKILL.md](SKILL.md) and its companions, newest first, each with the reason. Reasons cite findings in [RESEARCH.md](RESEARCH.md) (`R-`), lessons in [LEARNINGS.md](LEARNINGS.md) (`L-`), and test runs in [TESTS.md](TESTS.md) (`T-`). State in `evergreen.json`. Protocol: MAINTENANCE.md.

Entry shape: `### C-YYYYMMDD-n · date · one-line summary`, then `because:` (IDs or "user request"), `files:` (file and section), and a sentence on what changed. Cite section headings, not line numbers.

### C-20261021-1 · 2026-10-21 · decomp.py lookups, game recipes, key and data-file rules
- because: L-003, L-004, user request (the Playground Director mod; make the plugin cheaper in tokens and mistakes)
- files: scripts/decomp.py (new: decompile, keys, find, callers), SKILL.md (Step 5.1), references/game-recipes.md (new), references/harmony.md ("A mod with no patch")
- Step 5.1 points at decomp.py so an agent looks members up by line instead of reading whole decompiled files, and at the recipes found for freezing, killing, AI states, idle poses, walking to a point, blocking the attack click and saving a scene (deployed, not yet playtested). The reference picks keys by what runs in play and gives each feature its own component.

### C-20261018-2 · 2026-10-18 · Release AI target claims through the game; link the example mod
- because: L-002, T-20261018-2, user request (the mod goes in the repo as an example)
- files: references/harmony.md ("Doing what the game never does"), SKILL.md (Step 5.5)
- The reference says to release kill targets with `AI_TargetClaimer.UnclaimMyTarget` before moving a controller and never to null `myKillTarget` by hand, with the duplicate-claimant crash as evidence; Step 5 links examples/SwapUnaware as the worked example.

### C-20261018-1 · 2026-10-18 · Mods without patches, guarding states the game never reaches, crash stacks
- because: L-001, T-20261018-1, R-20261018-1
- files: SKILL.md (Step 5, Prove it), references/harmony.md (new sections "A mod with no patch" and "Doing what the game never does"; Failure modes)
- Step 5 now has the agent grep callers and ask for an edge-case playtest before calling a mod done; the reference shows a patch-free MonoBehaviour host, free keys, from-bytes loading (redeploy while running), room-scoped game queries and the KillMe example; Failure modes reads the new `stack` field of `MPN log`.


### C-20261017-6 · 2026-10-17 · Route level and map requests to madnesswright-scenes
- because: T-20261017-4 (madnesswright-scenes 0/6 on its trigger runs; the hub and modtools took them)
- files: SKILL.md (description)
- Says levels, maps and .sobj scenes belong to madnesswright-scenes, so a .sobj question no longer lands here.

### C-20261017-5 · 2026-10-17 · Name the install folder in Step 5.1
- because: T-20261017-3 (action-1 runs 2 and 3 both first tried `common/Madness Project Nexus/` and failed)
- files: SKILL.md (Step 5.1)
- Points at `install.install_dir` from `MPN where` and names the `Madness Project Nexus 2` folder.

### C-20261017-4 · 2026-10-17 · Target net48, not net472; build outside the mod folder
- because: T-20261017-3 (outcome-1 run 2: net472 failed MSB3274 against the game's net48 0Harmony.dll; net48 built; the build's obj/ folder landed in the mod)
- files: SKILL.md (Step 5.3), references/harmony.md (Project file, Framework rule)
- The framework follows the shipped Harmony's target; the project folder stays outside the mod so obj/*.json is never read as cards.

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
