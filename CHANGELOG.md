# Changelog

Plugin versions. Skill-level changes and their reasons are in each skill's own CHANGELOG.md.

## Unreleased

- `examples/SwapUnaware` 0.7.5: after a scene load teammates no longer fight each other (a respawned character now joins its saved squadmates' live squad; a None-faction squad of its own made them foes), and characters playing a looping animation (F4's or the level's) start it again. Load lines name each character's squad and faction; 2 s later Player.log names any saved squadmates now hostile. Scene files version 5. Played and passed on 2026-10-24.
- `examples/SwapUnaware` 0.7.6: the checks a moment after a scene load are skipped, with one log line, when the stage changed first (leaving the level within 2 s used to report every character off its spot or gone). Built and deployed, not yet played.
- madnesswright-scenes: `references/encounter-design.md` gains enemy roles (decided per card by watching it alone), an orchestration order, wave and squad pacing (one or two attacking at once, safe spawns, nothing new in the last wave, a release after it), finale and fairness rules and two playtest items; new `references/level-plan-template.md` (rooms, roles per card, waves, results, a campaign beat chart), after Rogers, *Level Up!*.
- madnesswright-modtools: `references/madobjects.md` "Before a new weapon": an attack matrix with a niche column and a hit-feedback check.
- Plugin icon resized from 1024 to 512 px (745 KB to 283 KB) to bring the tracked repository back from its size line.
- madnesswright-modtools: `references/madobjects.md` "From Blender": FBX settings for weapons and Character_Ref rigs (scale, leaf bones, one take per action, an axis test, why a Rigify rig needs a deform-only skeleton), checked in Blender 5.2.1.

## 0.1.8 · 2026-10-22

- `examples/SwapUnaware` 0.7.4: loading a scene in a story level or arena clears the blood, bullet holes, casings and debris made since the save (all of them after a restart of the stage or game); the Playground load already cleared them. Played in The Industrial Sector and published to the Workshop.
- `examples/SwapUnaware` 0.7.3: loading a scene in a story level or arena clears the bodies of everyone who died after the save (0.7.2 respawned them and left their old bodies too); squad, scripted, boss and essential bodies stay. The laborers' crate and other level props come back in their hands (their cards were not found by name), and enemies no longer walk off their spot on an order given after the save. Each load logs one line per character and, 2 s later, who has moved off their saved spot. Scene files version 4; older ones clear only the old bodies of the respawned.
- madnesswright-modtools: game-recipes lines on unregistered item cards and move orders.
- `examples/SwapUnaware` 0.7.2: a weapon you threw or dropped after saving comes back to your hand on load in story levels and arenas (0.7.1 cleared it with the floor weapons, so a thrown shotgun vanished). The same weapon is picked up from wherever it lies, or a fresh one is given if it is gone; this runs before the floor is cleared.
- `examples/SwapUnaware` 0.7.1: loading a scene in a story level or arena resets the ammo in your own weapons and in squadmates' career weapons (0.7.0 skipped both, so a fired shotgun stayed empty). The weapons themselves are not swapped, so the career's weapons stay its own. A reload in progress is cancelled so it cannot load over the saved count. Scenes saved in story mode with 0.7.0 hold no player weapons: save them again.
- `examples/SwapUnaware` 0.7.0: loading a scene resets weapons and items too, not only the characters. Scene files version 3 keep each character's hands, holsters and grenade with their ammo, every weapon lying loose in a room (put back where it lay; weapons dropped since the save are cleared), and in the Playground your own weapons. Career weapons and quest items are left alone. Older scene files load as before and say to save again. Loads cleanly in the game; playtest pending.
- madnesswright-modtools: a game-recipes line on loose weapons, loadouts and ammo.
- `examples/SwapUnaware` 0.6.1: renamed Director (was Playground Director), with text saying it works in every game mode; Workshop tag Playground dropped. No code change beyond the name.

## 0.1.7 · 2026-10-22

- `examples/SwapUnaware` 0.6.0: the Director menu (INSERT) lists every action with its key; features register their own lines, so new ones need no new button code. New keyless actions: clear corpses (keeps squad, scripted, boss and essential bodies) and clear blood, bullet holes, casings and debris. Loads cleanly in the game; playtest pending.
- madnesswright-modtools: a game-recipes line on clearing corpses.

## 0.1.6 · 2026-10-22

- `examples/SwapUnaware` 0.5.1: scene files keep their rooms and characters (`JsonUtility` had dropped the mod's own nested classes, so 0.4.0 and 0.5.0 saved empty scenes); `Game.Alive` no longer throws on a combatant the game has not set up. Playtested in a story level (The Industrial Sector): saves of 12 and 14 characters loaded back, with the dead respawned. madnesswright-modtools L-005 and a corrected game-recipes note.

- `examples/SwapUnaware` 0.5.0: F11 scenes in story levels, arenas, custom stages and hubs, not only the Playground. Loading matches the characters already in the level to the saved ones and puts them back, respawns the missing (serial numbers re-registered, wave units back in the wave), removes only unscripted extras, and moves you back to your saved room. Playground scenes from another Playground stage load that stage first. Scene files version 2; 0.4.0 files still load. Deployed, playtest pending.
- madnesswright-modtools: `references/game-recipes.md` gains "Levels, rooms and the career save".

## 0.1.5 · 2026-10-21

- madnesswright-modtools: new `scripts/decomp.py` (decompile, `keys` for which keys the game reads in play, `find` and `callers` for one-line member lookups) with `tests/test_decomp.py` in CI; new `references/game-recipes.md` (freeze, remove, playable roster, teams, AI states, idle poses, walking to a point, blocking the attack click, IMGUI windows, saving a scene); lessons L-003 (a key is a clash only if read in play) and L-004 (mod data outside `Mods/`).
- `examples/SwapUnaware` 0.4.0, now Playground Director: F1 clear, F2 freeze, F3 patrols, F4 idle animations, F9 guard, F10 alert cycle, F11 saved scenes, one component per feature. Playtested on the owner's PC (F9 and F11 not yet exercised) and published as an update of Workshop item 3822329165, which stayed public; `examples/WorkshopUpload`'s update path now proven.

## 0.1.4 · 2026-10-18

- madnesswright: the Workshop uploader moved to placenamehereig/MPN-Workshop-Uploader; content folder needs a dot in its name; the example mod is on the Workshop (item 3822329165); `examples/WorkshopUpload` publishes or updates an item from arguments, with no prompts.
- SwapUnaware 0.3.0 playtested on two PCs (modtools T-20261018-2).

- New `examples/SwapUnaware`: a complete Harmony mod (take control of any character, everyone else unaware) with its build and pack scripts and the bugs its playtests found; linked from the README and madnesswright-modtools.
- madnesswright-modtools: release an AI's target claims through `AI_TargetClaimer.UnclaimMyTarget` before moving a controller; never null `myKillTarget` by hand (L-002).

## 0.1.3 · 2026-10-18

- First beta launch (v2.20.a, build 25615196) and the first live Harmony mod built with madnesswright-modtools.
- madnesswright: no gear icon means not on the beta (the release build deletes the button); `MPN deploy --force` works while the game runs.
- `mpn.py log`: exception lines carry their `stack` frames, `mods_log.cards` lists OVERRIDE and FAILED card lines, and the from-bytes assembly line is no longer flagged.
- madnesswright-modtools: patch-free mods (a MonoBehaviour host), grep the callers of every game method a mod leans on and guard states the game never reaches, room-scoped game queries; first LEARNINGS entry.

## 0.1.2 · 2026-10-17

- Root `plugin.json` (Agent Plugins 1.0 shape) for Copilot CLI installs; kept equal to `.claude-plugin/plugin.json`.
- madnesswright-modtools Step 5.1 names the `Madness Project Nexus 2` install folder (testers guessed wrong twice).
- Doc corrections from the wiki run: `--strict`, `--data-dir` and `--steam` go before the command; mpn.py runs on Windows, macOS and Linux; modtools RESEARCH says net48; troubleshooting no longer claims check-mod detects nested folders.

## 0.1.1 · 2026-10-17

- Checked against the real game (release v1.11.a, build 25615280) and its decompiled mod loader: the mod log is `Mods/mods.log` (the official doc says `mods.txt`), the enabled list is `Mods/enabled.txt`, info can be `info.txt` or `info.json` with Name and Description required, `.json` cards only as a container, `.txt` Types case-sensitive, `IModEntrypoint` in the global namespace, Harmony 2.4.2 shipped, mods target net48.
- `mpn.py`: `where` and `log` report the build from Player.log, the enabled list, loaded mods and applied Harmony patches; `check-mod` follows the loader's rules and flags near-miss Roots.
- Harder eval fixtures, trigger routing between the three skills, release workflow, Dependabot for actions, README banner.

## 0.1.0 · 2026-10-17

- First version: `madnesswright` (mods, MadCards, branch, deploy, log, Workshop), `madnesswright-scenes` (SceneTools levels, stages, campaigns, arena, encounter design, playtest), `madnesswright-modtools` (ModTools MadObjects, Harmony assemblies).
- `mpn.py`: where, backup, check-mod, deploy, log, scene-lint (13 rules from the official SceneTools Overview).
- Creation pass research, eval suites, structure checks, size budget and CI.
