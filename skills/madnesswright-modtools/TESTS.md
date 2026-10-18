# Tests: madnesswright-modtools

Test runs for [SKILL.md](SKILL.md). Cases live in `evals/evals.json`. A failure that taught something is a lesson in [LEARNINGS.md](LEARNINGS.md); a fix it caused is logged in [CHANGELOG.md](CHANGELOG.md) with `because: T-...`; research it triggered is in [RESEARCH.md](RESEARCH.md); counts and the failing list are in `evergreen.json` under `tests`. Rules: MAINTENANCE.md (testing section) and the plugin's `protocol/TESTING.md`.

A test passes on evidence (a tool call in the trace, a file, a marker, a log line), never on the transcript's claim that something was done.

Entry shape: `### T-YYYYMMDD-n · date · harness · env · passed/total`, then one line per failing case (`id · kind · class · what the evidence showed`), then `led to:` (L-, C-, R- ids or none). Newest first. Budget 150 lines; archive older runs to `TESTS-ARCHIVE.md`.

## Runs

### T-20261018-2 · 2026-10-18 · live use by the owner · owner's PC and a second PC, beta 2.20.a · 4/4
- Mod 0.2.0 playtest: F8 worked; F5 failed on characters mid-fight (NullReferenceException in `AI_TargetClaimer.SetPriority` from `Controller_Base.LostConnection`), so dying in a taken body could not be tried. Cause: duplicate claimants left by F6's hand-written `myKillTarget = null` (L-002).
- Mod 0.3.0 (claims released through the game before every swap, claimants pruned): the owner reports everything worked; Player.log shows 7 swaps (one back home) and no exception or problem line; `applied_harmony` lists the id. Installed from the public Workshop item 3822329165 on a second PC and worked there too.
- live-1 · edge case · pass · mid-fight take-over; live-2 · pass · arena and F8; live-3 · pass · squadless spawn dying returns home; live-4 · pass · Workshop install on another PC.
- Open, not fixed: the owner thinks one of two grunts added to the player's own team turned on the player once (unconfirmed, no log signal; the owner chose to leave it).
- led to: L-002, C-20261018-2

### T-20261018-1 · 2026-10-18 · live use by the owner · owner's PC, beta 2.20.a · 1/2
- First live use: a swap-control and make-everyone-unaware mod built with this skill (Step 5). Build, check-mod, deploy, enable and load passed (`applied_harmony` listed the id; the mod's Initialize line in Player.log; swaps worked).
- live-1 · edge case · fail · taking over a Playground spawn and dying threw NullReferenceException in `Controller_Player.SwapControllersWithSquadmate` from `KillMe`; and swapping into a locked arena from outside left no way back (room-scoped target pick). Fixed in mod 0.2.0 with a Prefix and a screen-space pick; confirmed in T-20261018-2.
- led to: L-001, C-20261018-1, R-20261018-1


### T-20261017-4 · 2026-10-17 · headless `claude -p` per run, plugin installed as madnesswright@mark-local · dev-machine · 12/12 (trigger and decoy, 3 runs each)
- First pass on 0.1.0: 30/36 across the plugin. Every miss was madnesswright-scenes: its four trigger runs went to the hub ("Start here") or to madnesswright-modtools ("my mobj doesn't load" caught "I built the sobj"). The runner also stopped at the first Skill call.
- After C-20261017-6 (routing lines in all three descriptions) and a runner that records up to three Skill calls: 36/36; the scenes skill fired first on all six of its runs; every decoy went to its sibling or to no skill.
- This skill: trigger-1, trigger-2 3/3 each; decoy-1 (boss room level) went to madnesswright-scenes or no skill 3/3; decoy-2 (RimWorld) no skill 3/3.
- led to: C-20261017-6

### T-20261017-3 · 2026-10-17 · evergreen-tester subagents · dev-machine · 3/4 then 4/4 (value cases, runs 2 and 3)
- action-1 · pass ×2 · listed Managed and decompiled with ilspycmd before any [HarmonyPatch]; target `Item_Ranged.WeaponReloadSpeed` quoted with file and line from the decompile; info.txt with AssemblyPath, HarmonyID, EntryType. Both runs first guessed the install folder as `Madness Project Nexus` and had to look again.
- outcome-1 run 2 · fail on the skill's text, pass on the evidence · the skill said net472; the build failed MSB3274/MSB3275 against the game's net48 0Harmony.dll, the tester switched to net48; the project's obj/ landed inside the mod folder.
- outcome-1 run 3 (after C-20261017-4) · pass · net48 built first try; project in FastReload.Source/, the mod held only info.txt and the DLL; no game method named.
- outcome-1 baseline (no skill, game installed) · mostly pass · read Assembly-CSharp metadata itself and found IModEntrypoint and the ModItem keys, but called PatchAll inside Initialize (the game already runs PatchAll unless ManualPatch, so it patches twice), invented `UnpatchSelf` once, and left AssemblyPath and EntryType semantics unverified. With the game on disk this case separates the skill only weakly.
- led to: C-20261017-4, C-20261017-5

### T-20261017-2 · 2026-10-17 · evergreen-tester subagents · dev-machine · 2/2 (value cases, run 1 of 3)
- action-1 · pass · looked for the install, `Managed/` and `Assembly-CSharp.dll` and for ilspycmd before writing code (the game's files were not downloaded); the HarmonyPatch target was written as a labelled placeholder; info.txt had AssemblyPath, HarmonyID, EntryType.
- action-1 baseline (no skill) · fail · no lookup of Assembly-CSharp or Managed; used BepInEx 5 (`BaseUnityPlugin`) instead of the game's own loader; no info.txt. The skill made the difference.
- outcome-1 · pass · csproj with Private=false references from MPN_MANAGED and a provisional netstandard2.0 with the rule in a comment; IModEntrypoint entry class logging the HarmonyID; no game method named; check-mod's only error was the expected missing DLL.
- Trigger and decoy cases not run. Owed.
- led to: none

### T-20261017-1 · 2026-10-17 · not yet run · skill · 0/0
- Suite scaffolded; no run recorded. Write the cases in `evals/evals.json` (at least two trigger prompts, two decoys, one action case with evidence, one outcome case), run the baseline without the skill, then run with it (`evergreen-test`).
- led to: none
