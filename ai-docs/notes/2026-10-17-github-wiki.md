---
title: GitHub wiki written for 0.1.1, published for 0.1.2
kind: note
date: 2026-10-17
verified: 2026-10-17
stale_after: 2027-04-17
tags: [wiki, docs, 0.1.1, 0.1.2, github]
summary: "the wiki's pages, where the draft is and where its git working copy will be, how every example was verified against the published 0.1.1 release zip, the facts found on the way, the inaccuracies in the shipped docs, and how to publish and update the wiki; read before touching the wiki or the README sentences listed under inaccuracies"
---

# GitHub wiki for 0.1.1 and 0.1.2

Published 2026-10-17 as wiki commit eb1bedf (https://github.com/m4bwav/madnesswright/wiki), naming 0.1.2 in the footer, Home, Getting-Started and Versions. `live`: 10 pages, 0 failures, sidebar 13 of 13 probes, footer 1 of 1, anchors 13 links to 7 pages, 0 broken. 0.1.2 changed one docstring line of `mpn.py` against 0.1.1 (`git diff v0.1.1 v0.1.2 -- skills/madnesswright/scripts/mpn.py`), so the outputs printed by the 0.1.1 zip stand for 0.1.2; the 0.1.2 zip is 88,282 bytes, SHA-256 c851db83b706c2d6670bfa5bf532ba2a56ddc609c87b9723a0b2eedc706bfba5. Inaccuracies 1 to 6 below were fixed in 0.1.2; 7 and 8 were the session's own unfinished files, since committed.

## Summary

Wrote the GitHub wiki for m4bwav/madnesswright with the wikiwright skill (0.9.2): ten pages plus a sidebar and footer, written from the README, CHANGELOG, AGENTS.md, ai-docs, the three SKILL.md files with their references, RESEARCH.md (R-20261017-9 hub, R-20261017-6 modtools), TESTS.md, `mpn.py`, the tests, both workflows and the GitHub release. Every output on the pages was printed by the `mpn.py` inside the published `madnesswright-0.1.1.zip` and filled into the pages by script from the saved output. Gates: `check` 0 errors, `outputs` 27 checked 0 missing 1 skipped, `snippets` 0 missing (0 blocks of a checked language, 27 commands), everwrite `tells.py --wiki` 0 strong, 11 weak.

Pages: Home, Getting-Started, Mods-and-MadCards, Levels-with-SceneTools, Assets-and-Harmony, The-Helper, Troubleshooting, FAQ, Versions, Development, _Sidebar, _Footer.

Page set: no tested set exists for an agent-skills plugin; this follows wikiwright L-148 and L-156 and the roblox-agent wiki (Home, Getting started, one page per skill area, the helper's commands, troubleshooting, FAQ, versions, development). No API reference.

## Where the pages are

Draft: `madnesswright.wiki-draft`, a sibling of this clone, outside the repository (LF endings, plain markdown links to the page name without `.md`, no wikilinks). The working copy will be the sibling `madnesswright.wiki`, branch `master`, remote `https://github.com/m4bwav/madnesswright.wiki.git`.

The pages were generated from templates with `{{CASE:label}}` placeholders (the case's command in a `sh` block, its output in a `text` block, then "Exit code N."), filled from `2026-10-17-wiki-verify.out.txt` by a fill script in the session scratchpad, then `wikiwright.py unbs` for the Windows paths in prose. The fill script is not kept; to change an output, rerun the program and replace the block from the new output, or rebuild the same way.

## How it was published

The owner saved the first page on 2026-10-17; the pages were pushed from the sibling clone `../madnesswright.wiki`. Preflight state: `no-wiki-repo` (wiki feature on, `git ls-remote https://github.com/m4bwav/madnesswright.wiki.git` answered "Repository not found" on 2026-10-17). GitHub creates the wiki repository only when someone saves a first page at https://github.com/m4bwav/madnesswright/wiki/_new.

After the click:

1. `python <wikiwright>/scripts/wikiwright.py preflight m4bwav/madnesswright --clone ../madnesswright.wiki` from this clone (state `placeholder`).
2. Copy the draft's twelve files into the clone, `wikiwright.py check <clone> --version 0.1.1`, commit ("Wiki for madnesswright 0.1.1: ten pages, sidebar, footer"), `git push` (a plain fast-forward over the placeholder).
3. `wikiwright.py live m4bwav/madnesswright <clone>` and write its line here.

## How the examples were verified

- `2026-10-17-wiki-verify.py` (beside this note) downloads `madnesswright-0.1.1.zip` from the release (SHA-256 `b1e39ad3...b117`, 87,898 bytes; identical to the tree's `skills/` at tag v0.1.1), clones tag `v0.1.1` at depth 1 for `tests/fixtures` (the zip has no tests) and the development commands, and builds stand-in game folders in scratch: `steam/` with only `appmanifest_488860.acf` (build 25615280, BetaKey empty, then `releasebeta`), `game-data/` with Player.log, `Mods/`, `mods.log` and `enabled.txt` in the line shapes of `tests/test_mpn.py` and `references/troubleshooting.md`, and a SceneTools output. Every command that searches gets `--steam` and `--data-dir`, so nothing reads the real install. It masks the scratch folder (`<scratch>`), home (`~`), the backup stamp and unittest timing, and refuses to print if the home folder or user name survives as a path component.
- Saved output: `2026-10-17-wiki-verify.out.txt`, 29 sections, Windows 11, Python 3.14.6.
- Linux run: `2026-10-17-wiki-verify.linux.out.txt`, WSL Ubuntu, Python 3.14.4. `wikiwright.py diffout` shows only path separators and the order of `check-mod`'s `card_list` and warnings (filesystem listing order). The pages show the Windows run and say once that other systems print forward slashes.
- Repository tests, from the tag clone: `check_skills.py` ok (3 skills), `test_mpn.py` 26 passed, `budget.py` YELLOW (no red).

Not tested (marked on the pages where it matters):

- the Claude Code marketplace install from GitHub (the maintainer's trigger runs used a local marketplace);
- linking the zip's folders into each agent;
- `where` without overrides: the registry `SteamPath`, default Steam roots, `libraryfolders.vdf` and the default data folder;
- the Proton prefix data path on Linux or Steam Deck;
- anything against a real installed game, including a `mods.log` from a real Beta launch;
- macOS; Python 3.9 (CI covers both versions on three systems; this run used 3.14);
- the skills' agent behaviour (their own TESTS.md covers that), SceneTools or ModTools builds, the headless build, and building a Harmony assembly.

## Facts verified while writing (not in the README)

- `--strict`, `--data-dir` and `--steam` are options of `mpn.py` itself and go before the command name. `--strict` after it is an argparse usage error: usage text on stderr, exit 2, no JSON.
- Every command exits 0 whatever it finds unless `--strict` is given; an unexpected exception still prints one JSON object with the exception in `errors`. `MPN_DATA_DIR` overrides the data folder like `--data-dir`.
- `backup` writes to `~/mpn-backups` by default as `mpn-data-<YYYYMMDD-HHMMSS>.zip` and leaves out `Player.log` and `Player-prev.log`.
- `deploy --force` also deploys a mod that has errors; `mod` is printed as a full path; a folder already inside Mods is not copied (`note`).
- `check-mod` flags a misspelt Root only when it is within two edits of a Filename in the same mod (names differing only in digits are siblings); a misspelt vanilla Root with no similar Filename passes clean. It accepts `Stage_CAMPAIGN` in `.txt` cards and `Item` as a `.json` list name. It does not check where the folder sits or whether a DLL is a real assembly (a placeholder file passed).
- The order of `card_list` and of warnings follows the folder listing and differs between Windows and Linux.
- `scene-lint` reports S07, S10, S11 and S12 as warnings and the other nine rules as errors; it also warns on no `Room_Layout` instance and no Floor-tagged object; S12 counts every light in the scene file against four per room; floors inside room prefabs and unpacked rooms are not seen; a binary scene is refused with the Asset Serialization setting to change.
- `log` filters known clean-launch noise (`Shader is not supported on this GPU`, `TRANSITION SQUAD FAIL`, `Launch Parameters`, `[FAILED] ...exe`); `--lines` defaults to 30.
- `where`: `branch` reads `public (release)` with no beta key; `last_updated` is the manifest date in UTC; `tool_outputs` lists up to five newest builds per tool, in folders beside the data folder.
- The release zip holds `skills/`, README, LICENSE and SECURITY.md only: no `.claude-plugin` (it cannot be a marketplace) and no tests. The scenes and modtools skills call `../madnesswright/scripts/mpn.py`, so the three folders must stay side by side.
- `budget.py` is YELLOW at 0.1.1: longest description 1,000 characters (green 950), `mpn.py` 9,331 tokens (green 9,000), tracked repository 1,117 KB (green 512).
- 26 unit tests: scene-lint 8, check-mod 14, deploy and log 3, backup 1.
- 0.1.0 was never released or tagged; 0.1.1 was published 2026-10-18 01:54 UTC (the evening of 2026-10-17 locally), 0 downloads when read.

## Inaccuracies found in the shipped docs

None fixed here (the run was told not to edit README, CHANGELOG, skills or HANDOFF). Shipped files reach users only with a release.

1. README, "The helper" ("Each command prints one JSON object; add `--strict` to exit 1 when the result has errors"), the `mpn.py` docstring ("Every command prints exactly one JSON object") and SKILL.md ("Every command prints one JSON object"): `--strict` must come before the command name; written after it, argparse exits 2 with usage text and no JSON (`strict-after-command` case). Suggest "add `--strict` before the command".
2. `skills/madnesswright-modtools/RESEARCH.md`, Current understanding: "no `netstandard.dll`, so net472". SKILL.md Step 5.3, `references/harmony.md` and C-20261017-4 say net48 (net472 fails with MSB3274, T-20261017-3). The research summary was not updated with the fix.
3. `skills/madnesswright/references/madcards.md` lists the loader's Types without `Stage_CAMPAIGN`, which `mpn.py` accepts in `.txt` cards; `mpn.py` also accepts `Item` as a `.json` list name, which no doc mentions. Either the doc is incomplete or the helper is too lenient; the decompile notes would settle it.
4. `skills/madnesswright/references/troubleshooting.md` item 2 lists "the folder is not directly inside `Mods/`" among the causes and ends "`check-mod` says which". `check-mod` lints the folder it is given and cannot tell how deep it sits.
5. README, "The helper": "It runs on Windows, and on Linux or Steam Deck through Proton." `mpn.py` itself runs natively on Linux and macOS (its docstring and CI say so); it is the game that runs through Proton, and `mpn.py` looks inside the Proton prefix for the data folder.
6. `tests/check_skills.py` docstring says Python 3.8+; README, AGENTS.md and CI say 3.9+.
7. Not shipped, for the maintainer: `ai-docs/HANDOFF.md` and `ai-docs/log.md` still say 18 unit tests (26 at the tag), and HANDOFF's Owed list still has the real-game check (done, R-20261017-9) and a v0.1.0 release (never made; 0.1.1 was released instead).
8. Not shipped, uncommitted in the working tree on 2026-10-17 (another session's 0.1.2 bump): the new CHANGELOG 0.1.2 line "madnesswright-modtools Step 5.1 names the `Madness Project Nexus 2` install folder" describes a change 0.1.1 already ships (modtools C-20261017-5; the 0.1.1 zip's SKILL.md has it).

## Gotchas

- The tag `v0.1.1` is annotated; `git clone --branch v0.1.1` prints "is not a commit!" as a warning and works.
- The leak guard's first version fired on the GitHub owner `m4bwav`, which contains the Windows user name; it now matches the name only as a path component (wikiwright L-159).
- PowerShell `>` and the Bash tool's heredocs both risk the output: run the program from Git Bash with `>` (LF, no BOM), and write generator code with the editor tools (a hook refuses heredocs with a doubled backslash).

## Updating the wiki later

1. `git -C ../madnesswright.wiki pull --ff-only`.
2. Run `python ai-docs/notes/2026-10-17-wiki-verify.py X.Y.Z > <new>.out.txt` from Git Bash (it downloads the release zip and clones the tag itself), and the same under WSL with `python3` for the Linux file. Then `python <wikiwright>/scripts/wikiwright.py diffout ai-docs/notes/2026-10-17-wiki-verify.out.txt <new>.out.txt`; every difference is a block to replace.
3. `wikiwright.py outputs <wiki dir> <new>.out.txt --address ''`, `wikiwright.py snippets <wiki dir> ai-docs/notes/2026-10-17-wiki-verify.py`, `wikiwright.py check <wiki dir> --version X.Y.Z`, and everwrite `tells.py --wiki <wiki dir>/*.md`.
4. Commit, push, `wikiwright.py live m4bwav/madnesswright <wiki dir>`. Pages that name the version or the release: Home ("About these pages", release date), Getting-Started (`gh release download v0.1.1`, zip name, size and SHA-256), Mods-and-MadCards ("The Types the 0.1.1 references list"), The-Helper (the 0.1.1 zip), Versions (table and section), Development (tag clone, the budget's yellow rows, "How this wiki was checked"), _Footer.

Related: see also [../HANDOFF.md](../HANDOFF.md), [../log.md](../log.md), [2026-10-17-real-game-loader.md](2026-10-17-real-game-loader.md).
