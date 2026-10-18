# madnesswright

![A faceless grey stick-figure grunt in a black suit and tie ragdolls mid-fall against a cartoon spray of red in a bare grey concrete room under one hanging lamp, while a workbench in the foreground holds a laptop showing lines of code and a monitor with a grey-box level blockout](https://raw.githubusercontent.com/m4bwav/madnesswright/main/.github/images/banner.jpg)

Three self-maintaining ("evergreen") skills that let an AI coding agent (Claude Code, Codex, Cursor, Copilot) mod **MADNESS: Project Nexus** and build levels for it, the way the developer intends. They also check the work before it reaches the game. Written 2026-10-17 from a research pass over the game's Steam announcements, the developer's modding docs and tool projects, and the community's tools, then checked against an installed copy of the game and its own mod loader. Every claim is dated and sourced in each skill's `RESEARCH.md`, and each skill re-checks itself on a schedule.

Full documentation, with every helper command's real output: the [wiki](https://github.com/m4bwav/madnesswright/wiki).

## What the game offers (October 2026)

Project Nexus (GIBBING TREE, LLC, Steam app 488860) has real mod support. There is no level editor inside the game. Instead:

- **Mods** are folders under `%USERPROFILE%\AppData\LocalLow\Gibbing Tree\Madness Project Nexus\Mods\`. Each holds an `info.txt`, **MadCards** (JSON data for characters, weapons, waves, stages and more), and asset bundles. You turn them on from the gear icon on the main menu. The game logs mod problems to `Mods\mods.log` (the official doc calls it `mods.txt`).
- **Levels** are Unity scenes made in the developer's **SceneTools** project, on Unity 2019.2.6f1, and exported as `.sobj`. They can be Playground rooms, multi-stage campaigns with cutscenes, or Arena Siege and Infiltration maps.
- **New art** (weapons, armor, characters with custom rigs) goes through the **ModTools** project and is exported as `.mobj`.
- **Code**: since beta v2.19.a (April 2026) the game loads Harmony 2.4.2 assemblies named in `info.txt`. No BepInEx is needed.
- Official modding only works on the **`releasebeta`** Steam branch. The developer says to back up your saves before switching.

## The skills

| Skill | Job |
|---|---|
| [`madnesswright`](skills/madnesswright/SKILL.md) | Start here. Checks the branch and backs up saves. Writes `info.txt` and MadCards in either format. Lints, deploys and enables the mod, reads the game's mod log, and shares it on the Workshop. Also tells this game apart from the Flash classic and warns off the legacy DLL-swap routes. |
| [`madnesswright-scenes`](skills/madnesswright-scenes/SKILL.md) | Levels. Covers SceneTools setup and the room, waypoint, door, spawner, zone and blocker rules. Handles the Event System, stages, campaigns and arena maps. Plans encounters from level design practice, lints the scene, then runs the build, deploy and playtest loop. |
| [`madnesswright-modtools`](skills/madnesswright-modtools/SKILL.md) | MadObjects in ModTools: axes, grip points, Weapon Animator, Character_Ref. Also Harmony assemblies, which are written only after decompiling the game's own `Assembly-CSharp.dll`, with the target framework read from its `Managed` folder. |

## The helper

`skills/madnesswright/scripts/mpn.py` uses only the Python standard library (3.9+). It runs on Windows, macOS and Linux; the game itself runs on Windows, or on Linux and Steam Deck through Proton. Each command prints one JSON object and exits 0; put `--strict` before the command name (`mpn.py --strict check-mod <dir>`) to exit 1 when the result has errors. `--data-dir` and `--steam` also go before the command.

| Command | Does |
|---|---|
| `where` | Finds the Steam install, build, branch, download state, data and Mods folders, `mods.log`, the enabled list, `Player.log` and the build it ran, and the newest SceneTools and ModTools builds |
| `backup [--out DIR]` | Zips the game's data folder (careers, Mods, settings) |
| `check-mod <dir>` | Lints a mod folder: `info.txt`, curly quotes, strict `.json` against `.txt` shorthand, Filename and Type on every card, the assembly path, Workshop-id dependencies |
| `deploy <dir> [--force]` | Checks the mod, then copies it into the game's Mods folder |
| `log` | Reports the build, the enabled list, loaded mods and applied Harmony patches, and the problem lines and tail of `mods.log` and `Player.log` |
| `scene-lint <Scene.unity>` | Checks a SceneTools scene against 13 rules from the official docs |

## Install

Claude Code:

```
/plugin marketplace add m4bwav/madnesswright
/plugin install madnesswright@madnesswright
```

Other agents: copy the folders under `skills/` into the agent's skills folder (`.agents/skills/` or `.claude/skills/`).

You install Unity 2019.2.6f1, the SceneTools and ModTools projects (from https://github.com/MPN-Modders), a decompiler, and any Workshop uploader yourself. The skills never install software, and they never handle Steam credentials.

## Limits

- The agent cannot see the game. Evidence comes from the linters, the game's own log, and what you report after a playtest.
- MadCard field names come from the MadCard Maker glossary. The skills mark any name the official docs don't show as "confirm in MCM".
- The SceneTools and ModTools projects have no license file, so nothing from them is copied here. The test fixtures are synthetic.
- Not affiliated with Gibbing Tree or Krinkels. MADNESS: Project Nexus is their game.

## Privacy

The skills and the helper send nothing anywhere. `mpn.py` reads Steam's manifests and the game's data folder. It writes only a backup zip and, on `deploy`, the game's Mods folder. Research refreshes fetch public pages (Steam, Google Docs, GitHub).

## Development

Run `python tests/check_skills.py`, `python tests/test_mpn.py` and `python scripts/budget.py`. Rules for agents are in [AGENTS.md](AGENTS.md). Session notes are in [ai-docs/](ai-docs/HANDOFF.md).

License: MIT.
